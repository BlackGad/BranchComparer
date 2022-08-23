using System;
using System.Collections.Generic;
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
    private readonly ThrottlingTrigger _branchChangedTrigger;
    private readonly ILifetimeScope _scope;
    private IReadOnlyList<CommitViewModel> _commits;
    private string _error;

    public CommitsViewModel(FlowDirection flowDirection,
                            ISettingsService settingsService,
                            IGitService gitService,
                            ILifetimeScope scope)
    {
        _scope = scope;

        FlowDirection = flowDirection;
        GitService = gitService;
        //GitService.StateChanged += GitServiceOnStateChanged;

        //_branchChangedTrigger = ThrottlingTrigger.Setup()
        //                                         .Throttle(TimeSpan.FromMilliseconds(200))
        //                                         .Subscribe<EventArgs>(OnBranchChangedTriggered)
        //                                         .Create()
        //                                         .Activate();

        settingsService.LoadPopulateAndSaveOnDispose(GetType().AssemblyQualifiedName + flowDirection, this);
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
        //GitService.StateChanged -= GitServiceOnStateChanged;
        _branchChangedTrigger?.Dispose();
    }

    private void GitServiceOnStateChanged(object sender, EventArgs e)
    {
        _branchChangedTrigger.Trigger();
    }

    private CommitViewModel CreateCommitViewModel(Commit commit)
    {
        return _scope.Resolve<CommitViewModel>(TypedParameter.From(commit), TypedParameter.From(FlowDirection));
    }
}
