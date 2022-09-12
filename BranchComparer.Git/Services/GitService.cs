using BranchComparer.Git.Settings;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.GitService;
using LibGit2Sharp;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using Commit = BranchComparer.Infrastructure.Services.GitService.Commit;

namespace BranchComparer.Git.Services;

[DependencyRegisterAsInterface(typeof(IGitService))]
[DependencyRegisterAsSelf]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
public class GitService : IGitService
{
    private readonly IBroadcastService _broadcastService;
    private readonly ISettingsService _settingsService;

    public GitService(ISettingsService settingsService, IBroadcastService broadcastService)
    {
        _settingsService = settingsService;
        _broadcastService = broadcastService;
    }

    public IReadOnlyList<string> GetAvailableBranches()
    {
        var settings = _settingsService.GetSettings<GitSettings>();
        using var repo = new Repository(settings.RepositoryDirectory);
        return repo.Branches.Where(b => b.IsRemote).Select(b => b.FriendlyName).ToList();
    }

    public IReadOnlyList<Commit> GetCommits(string includeReachableFromBranchName, string excludeReachableFromBranchName)
    {
        if (string.IsNullOrEmpty(includeReachableFromBranchName))
        {
            return Array.Empty<Commit>();
        }

        var settings = _settingsService.GetSettings<GitSettings>();

        using var repo = new Repository(settings.RepositoryDirectory);

        var includeReachableFromBranch = repo.Branches[includeReachableFromBranchName];

        var commitFilter = new CommitFilter
        {
            IncludeReachableFrom = includeReachableFromBranch,
            SortBy = CommitSortStrategies.Time,
        };

        if (!string.IsNullOrEmpty(excludeReachableFromBranchName))
        {
            var excludeReachableFromBranch = repo.Branches[excludeReachableFromBranchName];
            commitFilter.ExcludeReachableFrom = excludeReachableFromBranch;
        }

        return repo.Commits
                   .QueryBy(commitFilter)
                   .Select(c => new Commit(
                               c.Id.Sha,
                               c.Author.Name,
                               c.Author.When,
                               c.Committer.Name,
                               c.Committer.When,
                               c.Message,
                               c.MessageShort))
                   .ToList();
    }

    public Uri GetPullRequestUri(int id)
    {
        var settings = _settingsService.GetSettings<GitSettings>();
        using var repo = new Repository(settings.RepositoryDirectory);

        var remote = repo.Network.Remotes.FirstOrDefault();
        if (string.IsNullOrEmpty(remote?.Url))
        {
            throw new InvalidOperationException("Repository is not connected to remote");
        }

        var builder = new UriBuilder(remote.Url)
        {
            UserName = string.Empty,
            Password = string.Empty,
        };
        builder.Path += "/pullrequest/" + id;
        return builder.Uri;
    }

    public void UpdateRemotes()
    {
        var settings = _settingsService.GetSettings<GitSettings>();
        using var repo = new Repository(settings.RepositoryDirectory);

        var options = new FetchOptions
        {
            Prune = true,
            TagFetchMode = TagFetchMode.Auto,
            CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials
            {
                Username = settings.Username ?? string.Empty,
                Password = settings.Password ?? string.Empty,
            },
        };

        var remote = repo.Network.Remotes.FirstOrDefault();
        if (remote == null)
        {
            return;
        }

        var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
        repo.Network.Fetch(remote.Name, refSpecs, options);

        _broadcastService.Broadcast(new RequireRefreshBranchesArgs());
    }
}
