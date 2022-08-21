using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.GitService;
using Newtonsoft.Json;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Git.ViewModels;

[DependencyRegisterAsSelf]
[JsonObject(MemberSerialization.OptIn)]
public class FilterViewModel : BaseNotifyPropertyChanged,
                               IViewModel
{
    private bool _isExpanded;

    public FilterViewModel(ISettingsService settingsService, IGitService gitService)
    {
        GitService = gitService;

        _isExpanded = true;

        settingsService.LoadPopulateAndSaveOnDispose(GetType().AssemblyQualifiedName, this);
    }

    public IGitService GitService { get; }

    [JsonProperty]
    public bool IsExpanded
    {
        get { return _isExpanded; }
        set { SetField(ref _isExpanded, value); }
    }
}
