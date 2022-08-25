using System.Windows;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.GitService;
using FluentValidation;
using LibGit2Sharp;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Threading;
using PS.WPF.Extensions;
using Commit = BranchComparer.Infrastructure.Services.GitService.Commit;

namespace BranchComparer.Git.Services.GitService;

[DependencyRegisterAsInterface(typeof(IGitService))]
[DependencyRegisterAsSelf]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
public class GitService : BaseNotifyPropertyChanged,
                          IGitService,
                          IDisposable
{
    private readonly IBroadcastService _broadcastService;
    private readonly IValidator<GitSettings> _settingsValidator;
    private GitSettings _settings;
    private ServiceState _state;

    public GitService(ISettingsService settingsService, IBroadcastService broadcastService, IValidator<GitSettings> settingsValidator)
    {
        _broadcastService = broadcastService;
        _settingsValidator = settingsValidator;

        _broadcastService.Subscribe<SettingsChangedArgs<GitSettings>>(OnSettingsChanged);

        ApplySettings(settingsService.GetSettings<GitSettings>());
    }

    public ServiceState State
    {
        get { return _state; }
        set
        {
            if (SetField(ref _state, value))
            {
                var eventType = typeof(ServiceStateChangedArgs<>).MakeGenericType(typeof(IGitService));
                var args = Activator.CreateInstance(eventType, _state);
                _broadcastService.Broadcast(eventType, args);
            }
        }
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        Application.Current.Dispatcher.Postpone(() => base.OnPropertyChanged(propertyName));
    }

    public void Dispose()
    {
        _broadcastService.Unsubscribe<SettingsChangedArgs<GitSettings>>(OnSettingsChanged);
    }

    public IReadOnlyList<string> GetAvailableBranches()
    {
        var settings = GetSettings();
        using var repo = new Repository(settings.RepositoryDirectory);
        return repo.Branches.Where(b => b.IsRemote).Select(b => b.FriendlyName).ToList();
    }

    public IReadOnlyList<Commit> GetCommits(string includeReachableFromBranchName, string excludeReachableFromBranchName)
    {
        if (string.IsNullOrEmpty(includeReachableFromBranchName))
        {
            return Array.Empty<Commit>();
        }

        var settings = GetSettings();

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

    public void InvalidateSettings()
    {
        ApplySettings(_settings);
    }

    private void ApplySettings(GitSettings settings)
    {
        var validationResult = Async.Run(async () => await _settingsValidator.ValidateAsync(settings));

        lock (this)
        {
            State = validationResult.IsValid
                ? new ServiceState(true, "Settings are valid")
                : new ServiceState(false, string.Join(Environment.NewLine, validationResult.Errors.Select(e => e.ErrorMessage)));

            _settings = settings;
        }
    }

    private GitSettings GetSettings()
    {
        lock (this)
        {
            if (State.IsValid != true)
            {
                throw new InvalidOperationException($"Settings are not valid. {State.Description}");
            }

            return _settings;
        }
    }

    private void OnSettingsChanged(SettingsChangedArgs<GitSettings> args)
    {
        ApplySettings(args.Settings);
    }
}
