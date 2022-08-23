using BranchComparer.Git.Services.GitService;
using BranchComparer.Infrastructure.Services;
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
    private bool _isExpanded;

    public SettingsViewModel(ISettingsService settingsService, GitService gitService)
    {
        GitService = gitService;

        _isExpanded = true;

        BrowseGitRepositoryFolderCommand = new RelayUICommand(BrowseGitRepositoryFolder);
        InvalidateGitSettingsCommand = new RelayUICommand(InvalidateGitSettings);

        settingsService.LoadPopulateAndSaveOnDispose(GetType().AssemblyQualifiedName, this);
        Settings = settingsService.GetObservableSettings<GitSettings>();
    }

    public RelayUICommand BrowseGitRepositoryFolderCommand { get; }

    public GitService GitService { get; }

    public RelayUICommand InvalidateGitSettingsCommand { get; }

    [JsonProperty]
    public bool IsExpanded
    {
        get { return _isExpanded; }
        set { SetField(ref _isExpanded, value); }
    }

    public GitSettings Settings { get; }

    private void BrowseGitRepositoryFolder()
    {
        var dialog = new OpenFolderDialog
        {
            DefaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            InitialFolder = Settings.RepositoryDirectory
        };

        if (dialog.ShowDialog())
        {
            Settings.RepositoryDirectory = dialog.SelectedFolder;
        }
    }

    private void InvalidateGitSettings()
    {
        GitService.InvalidateSettings();
    }
}
