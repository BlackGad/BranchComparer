using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BranchComparer.Infrastructure.Services.AzureService;
using BranchComparer.Infrastructure.Services.GitService;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns.Aware;
using PS.MVVM.Services;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class CommitViewModel : BaseNotifyPropertyChanged,
                               IUnloadedAware,
                               ILoadedAware
{
    private readonly IAzureService _azureService;
    private readonly IBroadcastService _broadcastService;
    private readonly Commit _commit;

    public CommitViewModel(Commit commit, IAzureService azureService, IBroadcastService broadcastService)
    {
        _commit = commit;
        _azureService = azureService;
        _broadcastService = broadcastService;

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
                            .Select(m => m.Groups[1].Value)
                            .Distinct()
                            .ToList();

        RelatedItemsMessage = string.Join(" ", RelatedItems);
    }

    public string DetailedMessage { get; }

    public string MergedPR { get; }

    public IReadOnlyList<string> RelatedItems { get; }

    public string RelatedItemsMessage { get; }

    public string ShortMessage { get; }

    public void Loaded()
    {
        _
    }

    public void Unloaded()
    {
    }
}
