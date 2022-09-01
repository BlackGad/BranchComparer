using System.Collections.Generic;
using BranchComparer.ViewModels;

namespace BranchComparer.Services.FilterService;

internal interface IFilterService
{
    IReadOnlyList<CommitViewModel> FilterCommits(IEnumerable<CommitViewModel> commits, IReadOnlyList<CommitCherryPickViewModel> cherryPicks);
}
