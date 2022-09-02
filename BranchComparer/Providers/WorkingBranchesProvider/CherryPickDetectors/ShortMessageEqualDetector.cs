using BranchComparer.Infrastructure.Services.GitService;
using PS.Extensions;
using PS.IoC.Attributes;

namespace BranchComparer.Providers.WorkingBranchesProvider.CherryPickDetectors;

[DependencyRegisterAsInterface(typeof(ICherryPickDetector))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class ShortMessageEqualDetector : ICherryPickDetector
{
    public IEnumerable<CommitCherryPick> Detect(IReadOnlyList<Commit> left, IReadOnlyList<Commit> right)
    {
        return left
               .Compare(right, commit => HashCode.Combine(commit.MessageShort))
               .PresentInBoth
               .Where(tuple => tuple.Item1.Id != tuple.Item2.Id)
               .Select(tuple => new CommitCherryPick(tuple.Item1, tuple.Item2));
    }
}