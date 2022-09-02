using PS.IoC.Attributes;
using PS.MVVM.Services;

namespace BranchComparer.Services;

[DependencyRegisterAsInterface(typeof(IBroadcastService))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class BroadcastService : PS.MVVM.Services.BroadcastService
{
}
