using System.Text.RegularExpressions;

namespace BranchComparer.Infrastructure.Services.GitService;

public record Commit
{
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
        MessageShort = messageShort;
        Message = message.Trim('\r', '\n', '\t', ' ');

        RelatedItems = Regex.Matches(message, @"#([0-9]+)")
                            .Where(m => m.Success)
                            .Select(m => int.TryParse(m.Groups[1].Value, out var parsed) ? parsed : int.MaxValue)
                            .Where(v => v != int.MaxValue)
                            .Distinct()
                            .ToList();

        MergedPRs = Regex.Matches(message, @"(Merged PR ([0-9]+):\s*)")
                         .Where(m => m.Success)
                         .Select(m => int.TryParse(m.Groups[2].Value, out var parsed) ? parsed : int.MaxValue)
                         .Where(v => v != int.MaxValue)
                         .Distinct()
                         .ToList();

        var extraCommitMatch = Regex.Match(message, @"^(Commit ([0-9a-fA-F]+):\s*)");
        if (extraCommitMatch.Success)
        {
            var value = extraCommitMatch.Groups[1].Value;
            MessageShort = MessageShort.Replace(value, string.Empty);
        }
    }

    public string Author { get; }

    public DateTimeOffset AuthorTime { get; }

    public string Committer { get; }

    public DateTimeOffset CommitterTime { get; }

    public string Id { get; }

    public IReadOnlyList<int> MergedPRs { get; }

    public string Message { get; }

    public string MessageShort { get; }

    public IReadOnlyList<int> RelatedItems { get; }
}
