using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.AzureService;
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

    public SettingsViewModel(ISettingsService settingsService,
                                  IAzureService azureService)
    {
        AzureService = azureService;

        BrowseAzureCacheDirectoryCommand = new RelayUICommand(BrowseAzureCacheDirectory);
        InvalidateAzureSettingsCommand = new RelayUICommand(InvalidateAzureSettings);

        settingsService.LoadPopulateAndSaveOnDispose(GetType().AssemblyQualifiedName, this);
    }

    public IAzureService AzureService { get; }

    public RelayUICommand BrowseAzureCacheDirectoryCommand { get; }

    public RelayUICommand InvalidateAzureSettingsCommand { get; }

    [JsonProperty]
    public bool IsExpanded
    {
        get { return _isExpanded; }
        set { SetField(ref _isExpanded, value); }
    }

    private void BrowseAzureCacheDirectory()
    {
        var dialog = new OpenFolderDialog
        {
            DefaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            InitialFolder = AzureService.Settings.CacheDirectory
        };

        if (dialog.ShowDialog())
        {
            AzureService.Settings.CacheDirectory = dialog.SelectedFolder;
        }
    }

    private void InvalidateAzureSettings()
    {
        AzureService.InvalidateSettings();
    }
}
