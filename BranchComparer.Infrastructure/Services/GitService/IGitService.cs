using BranchComparer.Infrastructure.Services.Abstract;

namespace BranchComparer.Infrastructure.Services.GitService;

public interface IGitService : IAbstractService<GitSettings>
{
    IReadOnlyList<string> AvailableBranches { get; }

    IReadOnlyList<Commit> GetCommitsFor(string tag);

    void RegisterBranchUsage(string tag, string branchName);
}
