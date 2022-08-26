using BranchComparer.Infrastructure.Services.EnvironmentService;

namespace BranchComparer.Infrastructure.Services;

public interface IFilterService
{
    IReadOnlyList<IEnvironmentCommit> FilterCommits(IEnumerable<IEnvironmentCommit> commits);
}
