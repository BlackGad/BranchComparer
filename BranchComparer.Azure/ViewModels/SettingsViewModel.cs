using BranchComparer.Azure.Settings;
using BranchComparer.Infrastructure;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.AzureService;
using Newtonsoft.Json;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;
using PS.Windows.Interop.Components;
using PS.WPF.Patterns.Command;

namespace BranchComparer.Azure.ViewModels;

[DependencyRegisterAsSelf]
[JsonObject(MemberSerialization.OptIn)]
public class SettingsViewModel : IViewModel
{
    private readonly IAzureService _azureService;
    private readonly IBusyService _busyService;

    public SettingsViewModel(ISettingsService settingsService, IAzureService azureService, IBusyService busyService)
    {
        _azureService = azureService;
        _busyService = busyService;

        BrowseAzureCacheDirectoryCommand = new RelayUICommand(BrowseAzureCacheDirectory);
        ClearCacheCommand = new RelayUICommand(ClearCache);

        Settings = settingsService.GetObservableSettings<AzureSettings>();
    }

    public RelayUICommand BrowseAzureCacheDirectoryCommand { get; }

    public RelayUICommand ClearCacheCommand { get; }

    public AzureSettings Settings { get; }

    private void BrowseAzureCacheDirectory()
    {
        var dialog = new OpenFolderDialog
        {
            DefaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            InitialFolder = Settings.CacheDirectory,
        };

        if (dialog.ShowDialog())
        {
            Settings.CacheDirectory = dialog.SelectedFolder;
        }
    }

    private async void ClearCache()
    {
        try
        {
            using (_busyService.Push("Clearing cache"))
            {
                await Task.Run(() => _azureService.ClearCache());
            }
        }
        catch (Exception e)
        {
            throw new NotificationException("Cannot clear cache", e);
        }
    }
}
