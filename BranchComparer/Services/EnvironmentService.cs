using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.GitService;
using Newtonsoft.Json;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Threading.ThrottlingTrigger;
using PS.WPF.Extensions;

namespace BranchComparer.Services;

[DependencyRegisterAsInterface(typeof(IEnvironmentService))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
[JsonObject(MemberSerialization.OptIn)]
internal class EnvironmentService : BaseNotifyPropertyChanged,
                                    IEnvironmentService,
                                    IDisposable
{
    private readonly IBroadcastService _broadcastService;
    private readonly IGitService _gitService;
    private readonly ThrottlingTrigger _updateAvailableBranchesTrigger;
    private readonly ThrottlingTrigger _updateFilterTrigger;

    private IReadOnlyList<string> _availableBranches;
    private string _leftBranch;
    private IReadOnlyList<Commit> _leftCommits;
    private TimeSpan? _period;
    private string _rightBranch;
    private IReadOnlyList<Commit> _rightCommits;
    private bool _showUniqueCommits;

    public EnvironmentService(ISettingsService settingsService, IGitService gitService, IBroadcastService broadcastService)
    {
        _gitService = gitService;
        _broadcastService = broadcastService;
        _broadcastService.Subscribe<ServiceStateChangedArgs<IGitService>>(GitServiceStateChanged);

        _updateFilterTrigger = ThrottlingTrigger.Setup()
                                                .Throttle(TimeSpan.FromMilliseconds(100))
                                                .Subscribe<EventArgs>(OnUpdateFilterTriggered)
                                                .Create()
                                                .Activate();

        _updateAvailableBranchesTrigger = ThrottlingTrigger.Setup()
                                                           .Throttle(TimeSpan.FromMilliseconds(100))
                                                           .Subscribe<EventArgs>(OnUpdateAvailableBranchesTriggered)
                                                           .Create()
                                                           .Activate();

        settingsService.LoadPopulateAndSaveOnDispose(GetType().AssemblyQualifiedName, this);

        _updateAvailableBranchesTrigger.Trigger();
    }

    [JsonProperty]
    public TimeSpan? Period
    {
        get { return _period; }
        set
        {
            if (SetField(ref _period, value))
            {
                _updateFilterTrigger.Trigger();
            }
        }
    }

    [JsonProperty]
    public bool ShowUniqueCommits
    {
        get { return _showUniqueCommits; }
        set
        {
            if (SetField(ref _showUniqueCommits, value))
            {
                _updateFilterTrigger.Trigger();
            }
        }
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        Application.Current.Dispatcher.Postpone(() => base.OnPropertyChanged(propertyName));
    }

    public void Dispose()
    {
        _broadcastService.Unsubscribe<ServiceStateChangedArgs<IGitService>>(GitServiceStateChanged);
        _updateAvailableBranchesTrigger.Dispose();
        _updateFilterTrigger.Dispose();
    }

    public IReadOnlyList<string> AvailableBranches
    {
        get { return _availableBranches; }
        private set { SetField(ref _availableBranches, value); }
    }

    [JsonProperty]
    public string LeftBranch
    {
        get { return _leftBranch; }
        set
        {
            if (SetField(ref _leftBranch, value))
            {
                _updateFilterTrigger.Trigger();
            }
        }
    }

    [JsonProperty]
    public string RightBranch
    {
        get { return _rightBranch; }
        set
        {
            if (SetField(ref _rightBranch, value))
            {
                _updateFilterTrigger.Trigger();
            }
        }
    }

    public IReadOnlyList<Commit> LeftCommits
    {
        get { return _leftCommits; }
        private set { SetField(ref _leftCommits, value); }
    }

    public IReadOnlyList<Commit> RightCommits
    {
        get { return _rightCommits; }
        private set { SetField(ref _rightCommits, value); }
    }

    private void OnUpdateAvailableBranchesTriggered(object sender, EventArgs e)
    {
        try
        {
            AvailableBranches = _gitService.GetAvailableBranches();
        }
        catch
        {
            AvailableBranches = Array.Empty<string>();
        }

        _updateFilterTrigger.Trigger();
    }

    private void OnUpdateFilterTriggered(object sender, EventArgs e)
    {
        try
        {
            IReadOnlyList<Commit> leftCommits;
            IReadOnlyList<Commit> rightCommits;
            if (ShowUniqueCommits)
            {
                leftCommits = _gitService.GetCommits(LeftBranch, RightBranch);
                rightCommits = _gitService.GetCommits(RightBranch, LeftBranch);
            }
            else
            {
                leftCommits = _gitService.GetCommits(LeftBranch, null);
                rightCommits = _gitService.GetCommits(RightBranch, null);
            }

            LeftCommits = FilterCommits(leftCommits).ToList();
            RightCommits = FilterCommits(rightCommits).ToList();
        }
        catch
        {
            LeftCommits = Array.Empty<Commit>();
            RightCommits = Array.Empty<Commit>();
        }
    }

    private IEnumerable<Commit> FilterCommits(IEnumerable<Commit> commits)
    {
        if (Period.HasValue)
        {
            var untilTime = DateTime.Now - Period.Value;
            commits = commits.Where(c => c.Time >= untilTime);
        }

        return commits;
    }

    private void GitServiceStateChanged(ServiceStateChangedArgs<IGitService> args)
    {
        _updateAvailableBranchesTrigger.Trigger();
    }
}
