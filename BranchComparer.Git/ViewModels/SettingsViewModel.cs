using BranchComparer.Git.Settings;
using BranchComparer.Infrastructure;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.GitService;
using Newtonsoft.Json;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;
using PS.Windows.Interop.Components;
using PS.WPF.Patterns.Command;

namespace BranchComparer.Git.ViewModels;

[DependencyRegisterAsSelf]
[JsonObject(MemberSerialization.OptIn)]
public class SettingsViewModel : BaseNotifyPropertyChanged,
                                 IViewModel
{
    private readonly IBusyService _busyService;
    private readonly IGitService _gitService;

    public SettingsViewModel(ISettingsService settingsService, IGitService gitService, IBusyService busyService)
    {
        _gitService = gitService;
        _busyService = busyService;
        BrowseGitRepositoryFolderCommand = new RelayUICommand(BrowseGitRepositoryFolder);
        UpdateRemoteCommand = new RelayUICommand(UpdateRemote);

        Settings = settingsService.GetObservableSettings<GitSettings>();
    }

    public RelayUICommand BrowseGitRepositoryFolderCommand { get; }

    public GitSettings Settings { get; }

    public RelayUICommand UpdateRemoteCommand { get; }

    private void BrowseGitRepositoryFolder()
    {
        var dialog = new OpenFolderDialog
        {
            DefaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            InitialFolder = Settings.RepositoryDirectory,
        };

        if (dialog.ShowDialog())
        {
            Settings.RepositoryDirectory = dialog.SelectedFolder;
        }
    }

    private async void UpdateRemote()
    {
        try
        {
            using (_busyService.Push("Updating repository remotes"))
            {
                await Task.Run(() => _gitService.UpdateRemotes());
            }
        }
        catch (Exception e)
        {
            throw new NotificationException("Cannot update remotes", e);
        }
    }
}
