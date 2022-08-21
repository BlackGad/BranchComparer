using System.Threading;
using BranchComparer.Infrastructure.Services;
using PS.IoC.Attributes;
using PS.WPF.Controls.BusyContainer;

namespace BranchComparer.Services;

[DependencyRegisterAsInterface(typeof(IBusyService))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class BusyService : StackBusyState,
                             IBusyService
{
    public BusyService()
    {
        SynchronizationContext = SynchronizationContext.Current;
    }
}
