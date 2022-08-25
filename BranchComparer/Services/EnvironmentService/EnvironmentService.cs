using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autofac;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.EnvironmentService;
using BranchComparer.Infrastructure.Services.GitService;
using BranchComparer.ViewModels;
using Newtonsoft.Json;
using PS;
using PS.Extensions;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Threading.ThrottlingTrigger;
using PS.WPF.Extensions;

namespace BranchComparer.Services.EnvironmentService;

[DependencyRegisterAsInterface(typeof(IEnvironmentService))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
[JsonObject(MemberSerialization.OptIn)]
internal class EnvironmentService : BaseNotifyPropertyChanged,
                                    IEnvironmentService,
                                    IDisposable
{
    private readonly IBroadcastService _broadcastService;
    private readonly IGitService _gitService;
    private readonly ILifetimeScope _scope;
    private readonly ThrottlingTrigger _updateAvailableBranchesTrigger;
    private readonly ThrottlingTrigger _updateFilterTrigger;

    private IReadOnlyList<string> _availableBranches;
    private string _leftBranch;
    private IReadOnlyList<IEnvironmentCommit> _leftCommits;
    private TimeSpan? _period;
    private string _rightBranch;
    private IReadOnlyList<IEnvironmentCommit> _rightCommits;
    private bool _showUniqueCommits;

    public EnvironmentService(
        ILifetimeScope scope,
        ISettingsService settingsService,
        IGitService gitService,
        IBroadcastService broadcastService)
    {
        _scope = scope;
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

    public IReadOnlyList<IEnvironmentCommit> LeftCommits
    {
        get { return _leftCommits; }
        private set { SetField(ref _leftCommits, value); }
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

    public IReadOnlyList<IEnvironmentCommit> RightCommits
    {
        get { return _rightCommits; }
        private set { SetField(ref _rightCommits, value); }
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
            IEnumerable<Commit> leftCommits;
            IEnumerable<Commit> rightCommits;
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

            leftCommits = FilterCommits(leftCommits).ToList();
            rightCommits = FilterCommits(rightCommits).ToList();

            var cherryPicks = leftCommits.Compare(rightCommits, commit => HashCode.Combine(commit.MessageShort, commit.Author, commit.AuthorTime))
                                         .PresentInBoth
                                         .Where(tuple => tuple.Item1.Id != tuple.Item2.Id)
                                         .ToList();
            var cherryPicksSources = new Dictionary<Commit, string>();
            var cherryPicksTargets = new Dictionary<Commit, string>();
            foreach (var tuple in cherryPicks)
            {
                if (tuple.Item1.CommitterTime > tuple.Item2.CommitterTime)
                {
                    cherryPicksSources.Add(tuple.Item2, tuple.Item1.Id);
                    cherryPicksTargets.Add(tuple.Item1, tuple.Item2.Id);
                }
                else
                {
                    cherryPicksSources.Add(tuple.Item1, tuple.Item2.Id);
                    cherryPicksTargets.Add(tuple.Item2, tuple.Item1.Id);
                }
            }

            LeftCommits = TransformCommits(leftCommits, cherryPicksSources, cherryPicksTargets);
            RightCommits = TransformCommits(rightCommits, cherryPicksSources, cherryPicksTargets);
        }
        catch
        {
            LeftCommits = Array.Empty<IEnvironmentCommit>();
            RightCommits = Array.Empty<IEnvironmentCommit>();
        }
    }

    private IEnumerable<Commit> FilterCommits(IEnumerable<Commit> commits)
    {
        if (Period.HasValue)
        {
            var untilTime = DateTime.Now - Period.Value;
            commits = commits.Where(c => c.AuthorTime >= untilTime);
        }

        return commits;
    }

    private void GitServiceStateChanged(ServiceStateChangedArgs<IGitService> args)
    {
        _updateAvailableBranchesTrigger.Trigger();
    }

    private IReadOnlyList<IEnvironmentCommit> TransformCommits(
        IEnumerable<Commit> commits,
        Dictionary<Commit, string> cherryPickSources,
        Dictionary<Commit, string> cherryPickTargets)
    {
        return commits.Select(c =>
        {
            cherryPickSources.TryGetValue(c, out var cherryPickSource);
            cherryPickTargets.TryGetValue(c, out var cherryPickTarget);
            return new EnvironmentCommitViewModel
            {
                Id = c.Id,
                Author = c.Author,
                Message = c.Message,
                ShortMessage = c.MessageShort,
                Time = c.AuthorTime,
                CherryPickSource = cherryPickSource,
                CherryPickTarget = cherryPickTarget,
                PR = c.MergedPR.HasValue ? _scope.Resolve<EnvironmentCommitPRViewModel>(TypedParameter.From(c.MergedPR.Value)) : null,
                RelatedItems = c.RelatedItems.Select(i => _scope.Resolve<EnvironmentCommitRelatedItemViewModel>(TypedParameter.From(i))).ToList(),
            };
        }).ToList();
    }
}
