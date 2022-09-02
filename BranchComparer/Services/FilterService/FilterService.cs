using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.GitService;
using BranchComparer.Settings;
using BranchComparer.ViewModels;
using PS.IoC.Attributes;

namespace BranchComparer.Services.FilterService;

[DependencyRegisterAsInterface(typeof(IFilterService))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class FilterService : IFilterService
{
    private readonly ISettingsService _settingsService;

    public FilterService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public IReadOnlyList<CommitViewModel> FilterCommits(IEnumerable<CommitViewModel> commits, IReadOnlyList<CommitCherryPick> cherryPicks)
    {
        var settings = _settingsService.GetSettings<FilterSettings>();
        if (settings.ExcludeCherryPicks)
        {
            commits = commits.Where(c => cherryPicks.All(p => p.SourceId != c.Commit.Id && p.TargetId != c.Commit.Id));
        }

        if (settings.Period.HasValue)
        {
            var untilTime = DateTime.Now - settings.Period.Value;
            commits = commits.Where(c => c.Commit.AuthorTime >= untilTime);
        }

        if (!string.IsNullOrEmpty(settings.Message))
        {
            commits = commits.Where(c => c.Commit.Message.Contains(settings.Message, StringComparison.InvariantCultureIgnoreCase));
        }

        return commits.ToList();
    }
}
