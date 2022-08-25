using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.EnvironmentService;
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
    public ShellViewModel(IBusyService busyService, IEnvironmentService environmentService)
    {
        BusyService = busyService;
        EnvironmentService = environmentService;

        Title = App.GetApplicationTitle();
    }

    public IBusyService BusyService { get; }

    public IEnvironmentService EnvironmentService { get; }

    public string Title { get; }
}
