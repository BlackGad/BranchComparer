using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.Abstract;
using BranchComparer.Infrastructure.Services.GitService;
using LibGit2Sharp;
using PS.Extensions;
using PS.IoC.Attributes;
using PS.WPF.Extensions;
using Commit = BranchComparer.Infrastructure.Services.GitService.Commit;

namespace BranchComparer.Git.Services;

[DependencyRegisterAsInterface(typeof(IGitService))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class GitService : AbstractService<GitSettings>,
                            IGitService
{
    private readonly ConcurrentDictionary<string, string> _usedBranches;
    private IReadOnlyList<string> _currentBranches;

    public GitService(ISettingsService settingsService)
        : base(settingsService)
    {
        _currentBranches = Array.Empty<string>();
        _usedBranches = new ConcurrentDictionary<string, string>();
    }

    protected override ValidationResult ValidateSettings(GitSettings settings)
    {
        try
        {
            if (string.IsNullOrEmpty(settings.RepositoryDirectory))
            {
                return new ValidationResult("Repository directory not set");
            }

            if (!Directory.Exists(settings.RepositoryDirectory))
            {
                return new ValidationResult($"{settings.RepositoryDirectory} not exist");
            }

            using var repo = new Repository(settings.RepositoryDirectory);
            AvailableBranches = repo.Branches.Where(b => b.IsRemote).Select(b => b.FriendlyName).ToList();
        }
        catch (Exception e)
        {
            return new ValidationResult(e.GetBaseException().Message);
        }

        return null;
    }

    public IReadOnlyList<string> AvailableBranches
    {
        get { return _currentBranches; }
        private set
        {
            _currentBranches = value;
            Application.Current.Dispatcher.Postpone(() => OnPropertyChanged(nameof(AvailableBranches)));
        }
    }

    public IReadOnlyList<Commit> GetCommitsFor(string tag)
    {
        if (!_usedBranches.TryGetValue(tag, out var branchName))
        {
            throw new InvalidOperationException("Branch name not set");
        }

        using var repo = new Repository(Settings.RepositoryDirectory);

        var commitFilter = new CommitFilter
        {
            IncludeReachableFrom = repo.Branches[branchName],
            SortBy = CommitSortStrategies.Time,
        };

        if (Settings.ShowUniqueCommits)
        {
            var excludeReachableFromBranch = _usedBranches.Values.ExceptBy(name => string.Equals(branchName, name)).FirstOrDefault();
            if (!string.IsNullOrEmpty(excludeReachableFromBranch))
            {
                commitFilter.ExcludeReachableFrom = repo.Branches[excludeReachableFromBranch];
            }
        }

        var log = repo.Commits.QueryBy(commitFilter).AsEnumerable();
        if (Settings.Period.HasValue)
        {
            var untilTime = DateTime.Now - Settings.Period.Value;
            log = log.Where(c => c.Committer.When >= untilTime);
        }

        return log.Select(c => new Commit(c.Id.Sha, c.Author.Name, c.Author.When, c.Message, c.MessageShort))
                  .ToList();
    }

    public void RegisterBranchUsage(string tag, string branchName)
    {
        _usedBranches.AddOrUpdate(tag, branchName, (_, _) => branchName);
    }
}
