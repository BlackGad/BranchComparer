namespace BranchComparer.Infrastructure.Services.EnvironmentService;

public interface IEnvironmentCommit
{
    string Author { get; }

    string Id { get; }

    string Message { get; }

    object PR { get; }

    IReadOnlyList<object> RelatedItems { get; }

    string ShortMessage { get; }

    DateTimeOffset Time { get; }
}
