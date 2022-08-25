namespace BranchComparer.Infrastructure.Services.EnvironmentService;

public interface IEnvironmentService
{
    IReadOnlyList<string> AvailableBranches { get; }

    string LeftBranch { get; set; }

    IReadOnlyList<IEnvironmentCommit> LeftCommits { get; }

    TimeSpan? Period { get; set; }

    string RightBranch { get; set; }

    IReadOnlyList<IEnvironmentCommit> RightCommits { get; }

    bool ShowUniqueCommits { get; set; }
}
