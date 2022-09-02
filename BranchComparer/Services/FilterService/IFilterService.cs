using BranchComparer.Infrastructure.Services.GitService;
using BranchComparer.ViewModels;

namespace BranchComparer.Services.FilterService;

internal interface IFilterService
{
    IReadOnlyList<CommitViewModel> FilterCommits(IEnumerable<CommitViewModel> commits, IReadOnlyList<CommitCherryPick> cherryPicks);
}
