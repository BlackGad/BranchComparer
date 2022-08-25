using System.Text.RegularExpressions;

namespace BranchComparer.Infrastructure.Services.GitService;

public record Commit
{
    public Commit(string id, string author, DateTimeOffset time, string message, string messageShort)
    {
        Id = id;
        Author = author;
        Time = time;
        MessageShort = messageShort;
        Message = message.Trim('\r', '\n', '\t', ' ');

        var mergedPRMatch = Regex.Match(message, @"^(Merged PR ([0-9]+):\s*)");
        if (mergedPRMatch.Success && int.TryParse(mergedPRMatch.Groups[2].Value, out var parsedMergedPR))
        {
            MergedPR = parsedMergedPR;
            MessageShort = MessageShort.Replace(mergedPRMatch.Groups[1].Value, string.Empty);
        }

        RelatedItems = Regex.Matches(message, @"#([0-9]+)")
                            .Where(m => m.Success)
                            .Select(m => int.TryParse(m.Groups[1].Value, out var parsed) ? parsed : int.MaxValue)
                            .Where(v => v != int.MaxValue)
                            .Distinct()
                            .ToList();
    }

    public string Author { get; }

    public string Id { get; }

    public int? MergedPR { get; }

    public string Message { get; }

    public string MessageShort { get; }

    public IReadOnlyList<int> RelatedItems { get; }

    public DateTimeOffset Time { get; }
};
