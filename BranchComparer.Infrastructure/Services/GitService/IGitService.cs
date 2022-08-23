namespace BranchComparer.Infrastructure.Services.GitService;

public interface IGitService
{
    IReadOnlyList<string> GetAvailableBranches();

    IReadOnlyList<Commit> GetCommits(string includeReachableFromBranchName, string excludeReachableFromBranchName);

    void InvalidateSettings();
}
