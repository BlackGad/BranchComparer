using BranchComparer.Azure.Services.AzureService;
using BranchComparer.Azure.Settings;
using BranchComparer.Infrastructure.Services;
using Newtonsoft.Json;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;
using PS.Windows.Interop.Components;
using PS.WPF.Patterns.Command;

namespace BranchComparer.Azure.ViewModels;

[DependencyRegisterAsSelf]
[JsonObject(MemberSerialization.OptIn)]
public class SettingsViewModel : BaseNotifyPropertyChanged,
                                 IViewModel
{
    private bool _isExpanded;

    public SettingsViewModel(ISettingsService settingsService, AzureService azureService)
    {
        AzureService = azureService;

        _isExpanded = true;

        BrowseAzureCacheDirectoryCommand = new RelayUICommand(BrowseAzureCacheDirectory);
        InvalidateAzureSettingsCommand = new RelayUICommand(InvalidateAzureSettings);

        settingsService.LoadPopulateAndSaveOnDispose(GetType().AssemblyQualifiedName, this);
        Settings = settingsService.GetObservableSettings<AzureSettings>();
    }

    public AzureService AzureService { get; }

    public RelayUICommand BrowseAzureCacheDirectoryCommand { get; }

    public RelayUICommand InvalidateAzureSettingsCommand { get; }

    [JsonProperty]
    public bool IsExpanded
    {
        get { return _isExpanded; }
        set { SetField(ref _isExpanded, value); }
    }

    public AzureSettings Settings { get; }

    private void BrowseAzureCacheDirectory()
    {
        var dialog = new OpenFolderDialog
        {
            DefaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            InitialFolder = Settings.CacheDirectory
        };

        if (dialog.ShowDialog())
        {
            Settings.CacheDirectory = dialog.SelectedFolder;
        }
    }

    private void InvalidateAzureSettings()
    {
        AzureService.InvalidateSettings();
    }
}
