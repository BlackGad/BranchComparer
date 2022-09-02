namespace BranchComparer.Infrastructure.Services.GitService;

public record CommitCherryPick
{
    public CommitCherryPick(Commit first, Commit second)
    {
        if (first.CommitterTime > second.CommitterTime)
        {
            SourceId = second.Id;
            TargetId = first.Id;
        }
        else
        {
            SourceId = first.Id;
            TargetId = second.Id;
        }
    }

    public string SourceId { get; init; }

    public string TargetId { get; init; }
}
