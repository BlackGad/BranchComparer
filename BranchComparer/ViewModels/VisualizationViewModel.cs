using BranchComparer.Infrastructure.Services;
using BranchComparer.Settings;
using Newtonsoft.Json;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class VisualizationViewModel : BaseNotifyPropertyChanged,
                                      IViewModel
{
    private bool _isExpanded;

    public VisualizationViewModel(ISettingsService settingsService)
    {
        _isExpanded = true;

        settingsService.LoadPopulateAndSaveOnDispose(GetType().AssemblyQualifiedName, this);
        Settings = settingsService.GetObservableSettings<VisualizationSettings>();
    }

    [JsonProperty]
    public bool IsExpanded
    {
        get { return _isExpanded; }
        set { SetField(ref _isExpanded, value); }
    }

    public VisualizationSettings Settings { get; }
}
