using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BranchComparer.Infrastructure.Services.AzureService;
using BranchComparer.Infrastructure.Services.GitService;
using PS;
using PS.IoC.Attributes;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class CommitViewModel : BaseNotifyPropertyChanged
{
    private readonly IAzureService _azureService;

    public CommitViewModel(Commit commit, IAzureService azureService)
    {
        _azureService = azureService;

        ShortMessage = commit.MessageShort;
        DetailedMessage = commit.Message.Trim('\r', '\n', '\t', ' ');

        var mergedPRMatch = Regex.Match(commit.Message, @"^(Merged PR ([0-9]+):\s*)");
        if (mergedPRMatch.Success)
        {
            MergedPR = mergedPRMatch.Groups[2].Value;
            ShortMessage = ShortMessage.Replace(mergedPRMatch.Groups[1].Value, string.Empty);
        }

        RelatedItems = Regex.Matches(commit.Message, @"#([0-9]+)")
                            .Where(m => m.Success)
                            .Select(m => int.TryParse(m.Groups[1].Value, out var parsed) ? parsed : int.MaxValue)
                            .Where(v => v != int.MaxValue)
                            .Distinct()
                            .Select(id => new AzureItem() { Id = id })
                            .ToList();
    }

    public string DetailedMessage { get; }

    public string MergedPR { get; }

    public IReadOnlyList<AzureItem> RelatedItems { get; }

    public string ShortMessage { get; }
}
