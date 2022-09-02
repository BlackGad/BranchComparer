using System.Text.RegularExpressions;
using BranchComparer.Infrastructure.Services.GitService;
using PS.IoC.Attributes;

namespace BranchComparer.Providers.WorkingBranchesProvider.CherryPickDetectors;

[DependencyRegisterAsInterface(typeof(ICherryPickDetector))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class ShortMessageCommitReferenceForm1Detector : ICherryPickDetector
{
    private static readonly Regex Regex = new("Cherry-picked from commit `([0-9a-fA-F]+)`");

    public IEnumerable<CommitCherryPick> Detect(IReadOnlyList<Commit> left, IReadOnlyList<Commit> right)
    {
        var leftTable = left.ToDictionary(c => c.Id.Substring(0, 8));
        var rightTable = right.ToDictionary(c => c.Id.Substring(0, 8));

        var leftFromRight = left.SelectMany(c => Regex.Matches(c.Message)
                                                      .Where(m => m.Success)
                                                      .Select(m => m.Groups[1].Value)
                                                      .Where(value => rightTable.ContainsKey(value))
                                                      .Select(value => new CommitCherryPick(c, rightTable[value])));

        foreach (var pick in leftFromRight)
        {
            yield return pick;
        }

        var rightFromLeft = right.SelectMany(c => Regex.Matches(c.Message)
                                                       .Where(m => m.Success)
                                                       .Select(m => m.Groups[1].Value)
                                                       .Where(value => leftTable.ContainsKey(value))
                                                       .Select(value => new CommitCherryPick(c, leftTable[value])));

        foreach (var pick in rightFromLeft)
        {
            yield return pick;
        }
    }
}
