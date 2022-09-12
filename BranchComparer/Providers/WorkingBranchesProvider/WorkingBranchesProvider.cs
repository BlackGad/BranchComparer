using System.Text;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using BranchComparer.Git.Settings;
using BranchComparer.Infrastructure;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.GitService;
using BranchComparer.Providers.WorkingBranchesProvider.CherryPickDetectors;
using BranchComparer.Settings;
using BranchComparer.ViewModels;
using Microsoft.VisualStudio.Services.Common;
using PS.Extensions;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Threading.ThrottlingTrigger;
using PS.WPF.Extensions;

namespace BranchComparer.Providers.WorkingBranchesProvider;

[DependencyRegisterAsSelf]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
[DependencyAutoActivate]
internal class WorkingBranchesProvider
{
    private readonly IBroadcastService _broadcastService;
    private readonly IBusyService _busyService;
    private readonly IReadOnlyList<ICherryPickDetector> _cherryPickDetectors;
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
        IModelResolverService modelResolverService,
        IEnumerable<ICherryPickDetector> cherryPickDetectors)
    {
        _busyService = busyService;
        _scope = scope;
        _settingsService = settingsService;
        _gitService = gitService;
        _broadcastService = broadcastService;
        _modelResolverService = modelResolverService;
        _cherryPickDetectors = cherryPickDetectors.Enumerate().ToList();

        _broadcastService.Subscribe<SettingsChangedArgs<BranchSettings>>(OnBranchSettingsChanged);
        _broadcastService.Subscribe<SettingsChangedArgs<GitSettings>>(OnGitSettingsChanged);
        _broadcastService.Subscribe<RequireRefreshBranchesArgs>(OnRefreshBranches);

        _updateModelsTrigger = ThrottlingTrigger.Setup()
                                                .Throttle(TimeSpan.FromMilliseconds(100))
                                                .Subscribe<EventArgs>(OnUpdateModelsTriggered)
                                                .Create()
                                                .Activate();
        _updateModelsTrigger.Trigger();
    }

    private void OnUpdateModelsTriggered(object sender, EventArgs e)
    {
        IReadOnlyList<CommitViewModel> leftBranch;
        IReadOnlyList<CommitViewModel> rightBranch;
        IReadOnlyList<CommitCherryPick> cherryPicks;

        var busyState = _busyService.Push("Resolving commits");

        try
        {
            var gitSettings = _settingsService.GetSettings<GitSettings>();
            var branchSettings = _settingsService.GetSettings<BranchSettings>();

            IReadOnlyList<Commit> leftCommits;
            IReadOnlyList<Commit> rightCommits;

            if (gitSettings.ShowUniqueCommits)
            {
                leftCommits = _gitService.GetCommits(branchSettings.Left, branchSettings.Right);
                rightCommits = _gitService.GetCommits(branchSettings.Right, branchSettings.Left);
            }
            else
            {
                leftCommits = _gitService.GetCommits(branchSettings.Left, null);
                rightCommits = _gitService.GetCommits(branchSettings.Right, null);
            }

            cherryPicks = DetectCherryPicks(leftCommits, rightCommits);

            leftBranch = TransformCommits(leftCommits, cherryPicks).ToList();
            rightBranch = TransformCommits(rightCommits, cherryPicks).ToList();
        }
        catch
        {
            leftBranch = Array.Empty<CommitViewModel>();
            rightBranch = Array.Empty<CommitViewModel>();
            cherryPicks = Array.Empty<CommitCherryPick>();
        }

        busyState.Title = "Rendering branches";
        var dispatcher = Application.Current.Dispatcher;
        dispatcher.SafeCall(() =>
        {
            _modelResolverService.Collection(ModelRegions.LEFT_BRANCH).Clear();
            _modelResolverService.Collection(ModelRegions.RIGHT_BRANCH).Clear();

            var cherryPicksCollection = _modelResolverService.Collection(ModelRegions.CHERRY_PICKS);
            cherryPicksCollection.Clear();
            cherryPicksCollection.AddRange(cherryPicks);
        });

        var batchSize = 13;

        using var leftBatchEnumerator = leftBranch.Batch(batchSize).GetEnumerator();
        using var rightBatchEnumerator = rightBranch.Batch(batchSize).GetEnumerator();

        while (true)
        {
            var leftAcquired = leftBatchEnumerator.MoveNext();
            var rightAcquired = rightBatchEnumerator.MoveNext();
            if (!leftAcquired && !rightAcquired)
            {
                break;
            }

            var leftBatch = leftAcquired ? leftBatchEnumerator.Current : Array.Empty<CommitViewModel>();
            var rightBatch = rightAcquired ? rightBatchEnumerator.Current : Array.Empty<CommitViewModel>();

            void UpdateBranchesBatch()
            {
                var leftBranchCollection = _modelResolverService.Collection(ModelRegions.LEFT_BRANCH);
                leftBranchCollection.AddRange(leftBatch);

                var rightBranchCollection = _modelResolverService.Collection(ModelRegions.RIGHT_BRANCH);
                rightBranchCollection.AddRange(rightBatch);

                var statusBuilder = new StringBuilder();
                statusBuilder.AppendLine($"Left branch: {leftBranchCollection.Count}/{leftBranch.Count}");
                statusBuilder.Append($"Right branch: {rightBranchCollection.Count}/{rightBranch.Count}");
                busyState.Description = statusBuilder.ToString();
            }

            dispatcher.Postpone(DispatcherPriority.Background, UpdateBranchesBatch);
        }

        void NotifyModelsUpdateFinished()
        {
            var args = new ModelsUpdatedArgs(new[]
            {
                ModelRegions.LEFT_BRANCH,
                ModelRegions.RIGHT_BRANCH,
                ModelRegions.CHERRY_PICKS,
            });

            _broadcastService.Broadcast(args);

            busyState.Dispose();
        }

        dispatcher.Postpone(DispatcherPriority.Background, NotifyModelsUpdateFinished);
    }

    private IReadOnlyList<CommitCherryPick> DetectCherryPicks(IReadOnlyList<Commit> leftEnvironmentCommits, IReadOnlyList<Commit> rightEnvironmentCommits)
    {
        return _cherryPickDetectors
               .SelectMany(detector => detector.Detect(leftEnvironmentCommits, rightEnvironmentCommits))
               .Distinct()
               .ToList();
    }

    private void OnBranchSettingsChanged(SettingsChangedArgs<BranchSettings> obj)
    {
        _updateModelsTrigger.Trigger();
    }

    private void OnGitSettingsChanged(SettingsChangedArgs<GitSettings> obj)
    {
        _updateModelsTrigger.Trigger();
    }

    private void OnRefreshBranches(RequireRefreshBranchesArgs obj)
    {
        _updateModelsTrigger.Trigger();
    }

    private IEnumerable<CommitViewModel> TransformCommits(IEnumerable<Commit> commits, IReadOnlyList<CommitCherryPick> cherryPicks)
    {
        return commits.Select(c =>
        {
            var referencedCherryPicks = cherryPicks.Where(cherry => cherry.SourceId == c.Id || cherry.TargetId == c.Id);
            return _scope.Resolve<CommitViewModel>(TypedParameter.From(c), TypedParameter.From(referencedCherryPicks));
        }).ToList();
    }
}
