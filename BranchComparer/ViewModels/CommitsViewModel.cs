using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autofac;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.GitService;
using Newtonsoft.Json;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;
using PS.Threading.ThrottlingTrigger;
using PS.WPF.Extensions;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
[JsonObject(MemberSerialization.OptIn)]
public class CommitsViewModel : BaseNotifyPropertyChanged,
                                IDisposable,
                                IViewModel
{
    private readonly ILifetimeScope _scope;
    private readonly ThrottlingTrigger _settingsChangedTrigger;
    private readonly string _tag;
    private string _branch;
    private IReadOnlyList<CommitViewModel> _commits;
    private string _error;

    public CommitsViewModel(string tag,
                            FlowDirection flowDirection,
                            ISettingsService settingsService,
                            IGitService gitService,
                            ILifetimeScope scope)
    {
        _tag = tag;
        _scope = scope;

        FlowDirection = flowDirection;
        GitService = gitService;
        GitService.StateChanged += GitServiceOnStateChanged;

        _settingsChangedTrigger = ThrottlingTrigger.Setup()
                                                   .Throttle(TimeSpan.FromMilliseconds(200))
                                                   .Subscribe<EventArgs>(OnSettingsChangedTriggered)
                                                   .Create()
                                                   .Activate();

        settingsService.LoadPopulateAndSaveOnDispose(GetType().AssemblyQualifiedName + _tag, this);
    }

    [JsonProperty]
    public string Branch
    {
        get { return _branch; }
        set
        {
            if (SetField(ref _branch, value))
            {
                GitService.RegisterBranchUsage(_tag, value);
                _settingsChangedTrigger.Trigger();
            }
        }
    }

    public IReadOnlyList<CommitViewModel> Commits
    {
        get { return _commits; }
        set
        {
            _commits = value;
            Application.Current.Dispatcher.Postpone(() => OnPropertyChanged(nameof(Commits)));
        }
    }

    public string Error
    {
        get { return _error; }
        set { SetField(ref _error, value); }
    }

    public FlowDirection FlowDirection { get; }

    public IGitService GitService { get; }

    public void Dispose()
    {
        GitService.StateChanged -= GitServiceOnStateChanged;
        _settingsChangedTrigger?.Dispose();
    }

    private void GitServiceOnStateChanged(object sender, EventArgs e)
    {
        _settingsChangedTrigger.Trigger();
    }

    private void OnSettingsChangedTriggered(object sender, EventArgs e)
    {
        try
        {
            var commits = GitService.GetCommitsFor(_tag);
            Commits = commits.Select(CreateCommitViewModel).ToList();
        }
        catch (Exception exception)
        {
            Error = exception.GetBaseException().Message;
        }
    }

    private CommitViewModel CreateCommitViewModel(Commit commit)
    {
        return _scope.Resolve<CommitViewModel>(TypedParameter.From(commit), TypedParameter.From(FlowDirection));
    }
}
