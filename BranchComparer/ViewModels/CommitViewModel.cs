using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using Autofac;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.AzureService;
using BranchComparer.Infrastructure.Services.GitService;
using BranchComparer.Settings;
using PS;
using PS.Extensions;
using PS.IoC.Attributes;
using PS.MVVM.Patterns.Aware;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Patterns.Aware;
using PS.WPF.Extensions;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class CommitViewModel : BaseNotifyPropertyChanged,
                               ILoadedAware,
                               IUnloadedAware,
                               IIsVisibleAware
{
    private readonly IAzureService _azureService;
    private readonly IBroadcastService _broadcastService;
    private readonly ILifetimeScope _scope;
    private IReadOnlyList<CommitPRViewModel> _prs;
    private IReadOnlyList<CommitRelatedItemViewModel> _relatedItems;

    public CommitViewModel(Commit commit,
                           IEnumerable<CommitCherryPick> cherryPicks,
                           ILifetimeScope scope,
                           ISettingsService settingsService,
                           IAzureService azureService,
                           IBroadcastService broadcastService)
    {
        _scope = scope;
        _azureService = azureService;
        _broadcastService = broadcastService;

        Commit = commit;
        CommitDetailsViewModel = _scope.Resolve<CommitDetailsViewModel>(TypedParameter.From(commit));

        CherryPicks = cherryPicks.Enumerate().ToArray();
        IsCherryPickPart = CherryPicks.Any();
        IsVisible = true;

        RelatedItems = new ObservableCollection<CommitRelatedItemViewModel>();

        VisualizationSettings = settingsService.GetObservableSettings<VisualizationSettings>();

        PRs = commit.MergedPRs.Select(i => _scope.Resolve<CommitPRViewModel>(TypedParameter.From(i))).ToList();
        RelatedItems = commit.RelatedItems.Select(i => _scope.Resolve<CommitRelatedItemViewModel>(TypedParameter.From(i))).ToList();

        var indexBuilder = new StringBuilder();
        indexBuilder.Append(commit.Message.ToLowerInvariant() + "|");
        indexBuilder.Append(commit.Id.ToLowerInvariant() + "|");
        indexBuilder.Append(commit.Author.ToLowerInvariant() + "|");
        indexBuilder.Append(commit.Committer.ToLowerInvariant() + "|");

        SearchIndex = indexBuilder.ToString();
    }

    public IReadOnlyList<CommitCherryPick> CherryPicks { get; }

    public Commit Commit { get; }

    public CommitDetailsViewModel CommitDetailsViewModel { get; }

    public bool IsCherryPickPart { get; }

    public IReadOnlyList<CommitPRViewModel> PRs
    {
        get { return _prs; }
        set { SetField(ref _prs, value); }
    }

    public IReadOnlyList<CommitRelatedItemViewModel> RelatedItems
    {
        get { return _relatedItems; }
        set { SetField(ref _relatedItems, value); }
    }

    public string SearchIndex { get; }

    public VisualizationSettings VisualizationSettings { get; }

    public bool IsVisible { get; set; }

    public void Loaded()
    {
        _broadcastService.Subscribe<AzureItemsResolvedArgs>(OnAzureItemResolved);

        var resolvedItems = _azureService.GetItems(RelatedItems.Select(i => i.Id));
        UpdateRelatedItems(resolvedItems);
    }

    public void Unloaded()
    {
        _broadcastService.Unsubscribe<AzureItemsResolvedArgs>(OnAzureItemResolved);
    }

    private void OnAzureItemResolved(AzureItemsResolvedArgs args)
    {
        Application.Current.Dispatcher.Postpone(() => UpdateRelatedItems(args.Items));
    }

    private void UpdateRelatedItems(IEnumerable<AzureItem> items)
    {
        var resolveComparison = items.Compare(RelatedItems, item => item.Id.GetHash(), model => model.Id.GetHash());
        resolveComparison.PresentInBoth.ForEach(t =>
        {
            t.Item2.ApplyResolvedInformation(t.Item1);
        });

        var taskParents = RelatedItems.Where(i => i.Type == AzureItemType.Task && i.ParentId.HasValue).Select(i => i.ParentId.Value);
        var taskParentsComparison = taskParents.Compare(RelatedItems, i => i.GetHash(), model => model.Id.GetHash());

        if (taskParentsComparison.PresentInFirstOnly.Any())
        {
            var additionalTaskParentItems = taskParentsComparison
                                            .PresentInFirstOnly
                                            .Select(i => _scope.Resolve<CommitRelatedItemViewModel>(TypedParameter.From(i)))
                                            .ToList();

            RelatedItems = additionalTaskParentItems.Union(RelatedItems).ToList();

            UpdateRelatedItems(_azureService.GetItems(taskParentsComparison.PresentInFirstOnly));
        }
    }
}
