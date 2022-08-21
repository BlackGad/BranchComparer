using PS.IoC.Attributes;
using PS.MVVM.Services;

namespace BranchComparer.Services;

[DependencyRegisterAsInterface(typeof(IViewResolverService))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class ViewResolverService : PS.MVVM.Services.ViewResolverService
{
}