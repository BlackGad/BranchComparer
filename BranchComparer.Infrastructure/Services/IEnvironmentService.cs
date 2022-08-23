using BranchComparer.Infrastructure.Services.GitService;

namespace BranchComparer.Infrastructure.Services;

public interface IEnvironmentService
{
    IReadOnlyList<string> AvailableBranches { get; }

    string LeftBranch { get; set; }

    IReadOnlyList<Commit> LeftCommits { get; }

    TimeSpan? Period { get; set; }

    string RightBranch { get; set; }

    IReadOnlyList<Commit> RightCommits { get; }

    bool ShowUniqueCommits { get; set; }
}
