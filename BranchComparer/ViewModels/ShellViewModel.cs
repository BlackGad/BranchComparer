using BranchComparer.Infrastructure.Services;
using BranchComparer.Settings;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;
using PS.Patterns.Aware;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class ShellViewModel : BaseNotifyPropertyChanged,
                              ITitleAware,
                              IViewModel
{
    public ShellViewModel(IBusyService busyService, ISettingsService settingsService)
    {
        BusyService = busyService;

        Title = App.GetApplicationTitle();
        BranchSettings = settingsService.GetObservableSettings<BranchSettings>();
    }

    public IBusyService BusyService { get; }

    public BranchSettings BranchSettings { get; }

    public string Title { get; }
}
