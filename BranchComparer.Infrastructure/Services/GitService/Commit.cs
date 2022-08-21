namespace BranchComparer.Infrastructure.Services.GitService;

public record Commit(string Id, string Author, DateTimeOffset Time, string Message, string MessageShort);
