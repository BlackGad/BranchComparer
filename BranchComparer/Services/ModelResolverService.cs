using PS.IoC.Attributes;
using PS.MVVM.Services;

namespace BranchComparer.Services;

[DependencyRegisterAsInterface(typeof(IModelResolverService))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class ModelResolverService : PS.MVVM.Services.ModelResolverService
{
}
