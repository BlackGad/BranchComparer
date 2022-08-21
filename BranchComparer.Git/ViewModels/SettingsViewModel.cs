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
    private bool _isExpanded;

    public SettingsViewModel(ISettingsService settingsService,
                                IGitService gitService)
    {
        GitService = gitService;

        BrowseGitRepositoryFolderCommand = new RelayUICommand(BrowseGitRepositoryFolder);
        InvalidateGitSettingsCommand = new RelayUICommand(InvalidateGitSettings);

        settingsService.LoadPopulateAndSaveOnDispose(GetType().AssemblyQualifiedName, this);
    }

    public RelayUICommand BrowseGitRepositoryFolderCommand { get; }

    public IGitService GitService { get; }

    public RelayUICommand InvalidateGitSettingsCommand { get; }

    public bool IsExpanded
    {
        get { return _isExpanded; }
        set { SetField(ref _isExpanded, value); }
    }

    private void BrowseGitRepositoryFolder()
    {
        var dialog = new OpenFolderDialog
        {
            DefaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            InitialFolder = GitService.Settings.RepositoryDirectory
        };

        if (dialog.ShowDialog())
        {
            GitService.Settings.RepositoryDirectory = dialog.SelectedFolder;
        }
    }

    private void InvalidateGitSettings()
    {
        GitService.InvalidateSettings();
    }
}
