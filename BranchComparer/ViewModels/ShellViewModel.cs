using BranchComparer.Infrastructure.Services;
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
    public ShellViewModel(IBusyService busyService)
    {
        BusyService = busyService;

        Title = App.GetApplicationTitle();
    }

    public IBusyService BusyService { get; }

    public string Title { get; }
}
