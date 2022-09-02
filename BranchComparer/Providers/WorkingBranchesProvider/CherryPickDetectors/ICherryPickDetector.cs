using BranchComparer.Infrastructure.Services.GitService;

namespace BranchComparer.Providers.WorkingBranchesProvider.CherryPickDetectors;

public interface ICherryPickDetector
{
    IEnumerable<CommitCherryPick> Detect(IReadOnlyList<Commit> left, IReadOnlyList<Commit> right);
}