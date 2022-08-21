using System.Text.RegularExpressions;
using BranchComparer.Infrastructure.Services.GitService;
using PS;
using PS.IoC.Attributes;

namespace BranchComparer.Git.ViewModels;

[DependencyRegisterAsSelf]
public class CommitViewModel : BaseNotifyPropertyChanged
{
    public CommitViewModel(Commit commit)
    {
        Commit = commit;
        Message = commit.MessageShort;

        var mergedPRMatch = Regex.Match(commit.Message, @"^(Merged PR ([0-9]+):\s*)");
        if (mergedPRMatch.Success)
        {
            MergedPR = mergedPRMatch.Groups[2].Value;
            Message = Message.Replace(mergedPRMatch.Groups[1].Value, string.Empty);
        }

        RelatedItems = Regex.Matches(commit.Message, @"#([0-9]+)")
                            .Where(m => m.Success)
                            .Select(m => m.Groups[1].Value)
                            .Distinct()
                            .ToList();

        RelatedItemsMessage = string.Join(" ", RelatedItems);
    }

    public Commit Commit { get; }

    public string MergedPR { get; }

    public string Message { get; }

    public IReadOnlyList<string> RelatedItems { get; }

    public string RelatedItemsMessage { get; }
}
