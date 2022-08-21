using BranchComparer.Infrastructure.Services.GitService;
using BranchComparer.Infrastructure.ViewModels;

namespace BranchComparer.Infrastructure.Services.ViewModelsService;

public interface IViewModelsService
{
    IReadOnlyList<CommitViewModel> CreateViewModels(string tag, IEnumerable<Commit> commits);
}
