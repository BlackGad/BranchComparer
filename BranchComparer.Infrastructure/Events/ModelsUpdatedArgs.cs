namespace BranchComparer.Infrastructure.Events;

public record ModelsUpdatedArgs(IReadOnlyList<string> Regions);
