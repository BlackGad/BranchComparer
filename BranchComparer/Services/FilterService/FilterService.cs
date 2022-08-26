using System;
using System.Collections.Generic;
using System.Linq;
using BranchComparer.Infrastructure.Services;
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

    public IReadOnlyList<CommitViewModel> FilterCommits(IEnumerable<CommitViewModel> commits)
    {
        var settings = _settingsService.GetSettings<FilterSettings>();
        if (settings.Period.HasValue)
        {
            var untilTime = DateTime.Now - settings.Period.Value;
            commits = commits.Where(c => c.Time >= untilTime);
        }

        return commits.ToList();
    }
}