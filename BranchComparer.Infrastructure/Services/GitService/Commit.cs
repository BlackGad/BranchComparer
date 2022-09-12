using System.Text.RegularExpressions;

namespace BranchComparer.Infrastructure.Services.GitService;

public record Commit
{
    private static readonly IReadOnlyList<string> GarbageText = new[]
    {
        "feat(workplan):",
    };

    public Commit(
        string id,
        string author,
        DateTimeOffset authorTime,
        string committer,
        DateTimeOffset committerTime,
        string message,
        string messageShort)
    {
        Id = id;
        Author = author;
        AuthorTime = authorTime;
        Committer = committer;
        CommitterTime = committerTime;

        Message = message.Trim('\r', '\n', '\t', ' ');
        MessageShort = messageShort.Trim('\r', '\n', '\t', ' ');

        var relatedItemsMatches = Regex.Matches(message, @"#([0-9]+)")
                                       .Where(m => m.Success)
                                       .ToList();
        RelatedItems = relatedItemsMatches
                       .Select(m => int.TryParse(m.Groups[1].Value, out var parsed) ? parsed : int.MaxValue)
                       .Where(v => v != int.MaxValue)
                       .Distinct()
                       .ToList();

        var mergedPRMatches = Regex.Matches(message, @"(Merged PR ([0-9]+):\s*)")
                                   .Where(m => m.Success)
                                   .ToList();

        MergedPRs = mergedPRMatches
                    .Select(m => int.TryParse(m.Groups[2].Value, out var parsed) ? parsed : int.MaxValue)
                    .Where(v => v != int.MaxValue)
                    .Distinct()
                    .ToList();

        var extraCommitMatch = Regex.Matches(message, @"^(Commit ([0-9a-fA-F]+):\s*)")
                                    .Where(m => m.Success)
                                    .ToList();

        var extraTextToDelete = Enumerable.Empty<string>()
                                          .Union(GarbageText)
                                          .Union(relatedItemsMatches.Select(m => m.Groups[0].Value))
                                          .Union(mergedPRMatches.Select(m => m.Groups[1].Value))
                                          .Union(extraCommitMatch.Select(m => m.Groups[1].Value));

        MessageSanitized = MessageShort;
        foreach (var extraText in extraTextToDelete)
        {
            MessageSanitized = MessageSanitized.Replace(extraText, string.Empty);
        }
        MessageSanitized = MessageSanitized.Trim('\r', '\n', '\t', ' ');
    }

    public string Author { get; }

    public DateTimeOffset AuthorTime { get; }

    public string Committer { get; }

    public DateTimeOffset CommitterTime { get; }

    public string Id { get; }

    public IReadOnlyList<int> MergedPRs { get; }

    public string Message { get; }

    public string MessageSanitized { get; }

    public string MessageShort { get; }

    public IReadOnlyList<int> RelatedItems { get; }
}
