using BranchComparer.Infrastructure;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Settings;
using BranchComparer.ViewModels;
using PS.Extensions;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Threading.ThrottlingTrigger;

namespace BranchComparer.Services;

[DependencyRegisterAsSelf]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
[DependencyAutoActivate]
internal class FilterService
{
    private readonly IBroadcastService _broadcastService;
    private readonly ThrottlingTrigger _filterRequiredTrigger;
    private readonly IModelResolverService _modelResolverService;
    private readonly ISettingsService _settingsService;

    public FilterService(IModelResolverService modelResolverService,
                         IBroadcastService broadcastService,
                         ISettingsService settingsService)
    {
        _modelResolverService = modelResolverService;
        _broadcastService = broadcastService;
        _settingsService = settingsService;

        _broadcastService.Subscribe<SettingsChangedArgs<FilterSettings>>(OnSettingsChanged);
        _broadcastService.Subscribe<ModelsUpdatedArgs>(OnModelsUpdated);

        _filterRequiredTrigger = ThrottlingTrigger.Setup()
                                                  .Throttle(TimeSpan.FromMilliseconds(100))
                                                  .Subscribe<EventArgs>(OnFilterRequiredTrigger)
                                                  .Create()
                                                  .Activate();
    }

    private void OnFilterRequiredTrigger(object sender, EventArgs e)
    {
        var settings = _settingsService.GetSettings<FilterSettings>();
        var searchString = settings.Message.ToLowerInvariant();

        bool CalculateVisibility(CommitViewModel commit)
        {
            var visibility = true;

            if (settings.ExcludeCherryPicks)
            {
                visibility = !commit.CherryPicks.Any();
            }

            if (settings.Period.HasValue)
            {
                var untilTime = DateTime.Now - settings.Period.Value;
                visibility = visibility && commit.Commit.AuthorTime < untilTime;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                visibility = visibility && commit.SearchIndex.Contains(searchString);
            }

            return visibility;
        }

        var changedCommits = Enumerable.Empty<CommitViewModel>()
                                       .Union(_modelResolverService.Collection(ModelRegions.LEFT_BRANCH).Enumerate<CommitViewModel>())
                                       .Union(_modelResolverService.Collection(ModelRegions.RIGHT_BRANCH).Enumerate<CommitViewModel>())
                                       .AsParallel()
                                       .Select(c => new
                                       {
                                           Commit = c,
                                           Current = c.IsVisible,
                                           Calculated = CalculateVisibility(c),
                                       })
                                       .Where(selection => selection.Current != selection.Calculated)
                                       .ToList();

        if (changedCommits.Any())
        {
            foreach (var changedCommit in changedCommits)
            {
                changedCommit.Commit.IsVisible = changedCommit.Calculated;
            }

            _broadcastService.Broadcast(new RequireRefreshBranchFilterViewsArgs());
        }
    }

    private void OnModelsUpdated(ModelsUpdatedArgs args)
    {
        if (args.Regions.Contains(ModelRegions.LEFT_BRANCH) || args.Regions.Contains(ModelRegions.RIGHT_BRANCH))
        {
            _filterRequiredTrigger.Trigger();
        }
    }

    private void OnSettingsChanged(SettingsChangedArgs<FilterSettings> args)
    {
        _filterRequiredTrigger.Trigger();
    }
}
