using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autofac;
using BranchComparer.Infrastructure;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.GitService;
using BranchComparer.Services.FilterService;
using BranchComparer.Settings;
using BranchComparer.ViewModels;
using PS.Extensions;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Threading.ThrottlingTrigger;
using PS.WPF.Extensions;

namespace BranchComparer.Providers;

[DependencyRegisterAsSelf]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
[DependencyAutoActivate]
internal class WorkingBranchesProvider : IDisposable
{
    private readonly IBroadcastService _broadcastService;
    private readonly IBusyService _busyService;
    private readonly IFilterService _filterService;
    private readonly IGitService _gitService;
    private readonly IModelResolverService _modelResolverService;
    private readonly ILifetimeScope _scope;
    private readonly ISettingsService _settingsService;
    private readonly ThrottlingTrigger _updateModelsTrigger;

    public WorkingBranchesProvider(
        IBusyService busyService,
        ILifetimeScope scope,
        ISettingsService settingsService,
        IGitService gitService,
        IBroadcastService broadcastService,
        IFilterService filterService,
        IModelResolverService modelResolverService)
    {
        _busyService = busyService;
        _scope = scope;
        _settingsService = settingsService;
        _gitService = gitService;
        _broadcastService = broadcastService;
        _filterService = filterService;
        _modelResolverService = modelResolverService;

        _broadcastService.Subscribe<ServiceStateChangedArgs<IGitService>>(GitServiceStateChanged);
        _broadcastService.Subscribe<SettingsChangedArgs<FilterSettings>>(OnFilterSettingsChanged);
        _broadcastService.Subscribe<SettingsChangedArgs<BranchSettings>>(OnBranchSettingsChanged);

        _updateModelsTrigger = ThrottlingTrigger.Setup()
                                                .Throttle(TimeSpan.FromMilliseconds(100))
                                                .Subscribe<EventArgs>(OnUpdateModelsTriggered)
                                                .Create()
                                                .Activate();
        _updateModelsTrigger.Trigger();
    }

    void IDisposable.Dispose()
    {
        _broadcastService.Unsubscribe<ServiceStateChangedArgs<IGitService>>(GitServiceStateChanged);
        _broadcastService.Unsubscribe<SettingsChangedArgs<FilterSettings>>(OnFilterSettingsChanged);
        _broadcastService.Unsubscribe<SettingsChangedArgs<BranchSettings>>(OnBranchSettingsChanged);

        _updateModelsTrigger.Dispose();
    }

    private void OnUpdateModelsTriggered(object sender, EventArgs e)
    {
        IReadOnlyList<CommitViewModel> leftBranch;
        IReadOnlyList<CommitViewModel> rightBranch;
        IReadOnlyList<CommitCherryPickViewModel> cherryPicks;

        var busyState = _busyService.Push("Resolving commits");

        try
        {
            var commitsFilterSettings = _settingsService.GetSettings<FilterSettings>();
            var branchSettings = _settingsService.GetSettings<BranchSettings>();

            IEnumerable<Commit> leftCommits;
            IEnumerable<Commit> rightCommits;

            if (commitsFilterSettings.ShowUniqueCommits)
            {
                leftCommits = _gitService.GetCommits(branchSettings.Left, branchSettings.Right);
                rightCommits = _gitService.GetCommits(branchSettings.Right, branchSettings.Left);
            }
            else
            {
                leftCommits = _gitService.GetCommits(branchSettings.Left, null);
                rightCommits = _gitService.GetCommits(branchSettings.Right, null);
            }

            var leftEnvironmentCommits = TransformCommits(leftCommits);
            var rightEnvironmentCommits = TransformCommits(rightCommits);

            cherryPicks = leftEnvironmentCommits.Compare(rightEnvironmentCommits, commit => HashCode.Combine(commit.ShortMessage))
                                                .PresentInBoth
                                                .Where(tuple => tuple.Item1.Id != tuple.Item2.Id)
                                                .Select(tuple =>
                                                {
                                                    var first = tuple.Item1;
                                                    var second = tuple.Item2;

                                                    var viewModel = _scope.Resolve<CommitCherryPickViewModel>();
                                                    if (first.CommitterTime > second.CommitterTime)
                                                    {
                                                        viewModel.Source = second;
                                                        viewModel.Target = first;
                                                    }
                                                    else
                                                    {
                                                        viewModel.Source = first;
                                                        viewModel.Target = second;
                                                    }

                                                    first.AddCherryPickReference(viewModel);
                                                    second.AddCherryPickReference(viewModel);

                                                    return viewModel;
                                                })
                                                .ToList();

            leftBranch = _filterService.FilterCommits(leftEnvironmentCommits);
            rightBranch = _filterService.FilterCommits(rightEnvironmentCommits);
        }
        catch
        {
            leftBranch = Array.Empty<CommitViewModel>();
            rightBranch = Array.Empty<CommitViewModel>();
            cherryPicks = Array.Empty<CommitCherryPickViewModel>();
        }
        finally
        {
            busyState.Dispose();
        }

        Application.Current.Dispatcher.Postpone(() =>
        {
            _modelResolverService.Object(Regions.LEFT_BRANCH_COUNT).Value = leftBranch.Count;
            _modelResolverService.Object(Regions.RIGHT_BRANCH_COUNT).Value = rightBranch.Count;

            var leftBranchCollection = _modelResolverService.Collection(Regions.LEFT_BRANCH);
            leftBranchCollection.Clear();
            leftBranchCollection.AddRange(leftBranch);

            var rightBranchCollection = _modelResolverService.Collection(Regions.RIGHT_BRANCH);
            rightBranchCollection.Clear();
            rightBranchCollection.AddRange(rightBranch);

            var cherryPicksCollection = _modelResolverService.Collection(Regions.CHERRY_PICKS);
            cherryPicksCollection.Clear();
            cherryPicksCollection.AddRange(cherryPicks);
        });
    }

    private void GitServiceStateChanged(ServiceStateChangedArgs<IGitService> args)
    {
        _updateModelsTrigger.Trigger();
    }

    private void OnBranchSettingsChanged(SettingsChangedArgs<BranchSettings> obj)
    {
        _updateModelsTrigger.Trigger();
    }

    private void OnFilterSettingsChanged(SettingsChangedArgs<FilterSettings> args)
    {
        _updateModelsTrigger.Trigger();
    }

    private IReadOnlyList<CommitViewModel> TransformCommits(IEnumerable<Commit> commits)
    {
        return commits.Select(c => new CommitViewModel
        {
            Id = c.Id,
            Author = c.Author,
            Message = c.Message,
            ShortMessage = c.MessageShort,
            AuthorTime = c.AuthorTime,
            CommitterTime = c.CommitterTime,
            PR = c.MergedPR.HasValue ? _scope.Resolve<CommitPRViewModel>(TypedParameter.From(c.MergedPR.Value)) : null,
            RelatedItems = c.RelatedItems.Select(i => _scope.Resolve<CommitRelatedItemViewModel>(TypedParameter.From(i))).ToList(),
        }).ToList();
    }
}
