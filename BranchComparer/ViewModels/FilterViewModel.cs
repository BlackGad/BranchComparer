using BranchComparer.Infrastructure.Services;
using BranchComparer.Settings;
using Newtonsoft.Json;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
[JsonObject(MemberSerialization.OptIn)]
public class FilterViewModel : BaseNotifyPropertyChanged,
                               IViewModel
{
    private bool _isExpanded;

    public FilterViewModel(ISettingsService settingsService)
    {
        _isExpanded = true;

        settingsService.LoadPopulateAndSaveOnDispose(GetType().AssemblyQualifiedName, this);
        Settings = settingsService.GetObservableSettings<FilterSettings>();
    }

    [JsonProperty]
    public bool IsExpanded
    {
        get { return _isExpanded; }
        set { SetField(ref _isExpanded, value); }
    }

    public FilterSettings Settings { get; }
}
