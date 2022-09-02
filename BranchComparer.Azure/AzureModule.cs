using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using BranchComparer.Azure.ViewModels;
using BranchComparer.Azure.Views;
using BranchComparer.Infrastructure;
using PS.IoC.Extensions;
using PS.MVVM.Extensions;
using PS.MVVM.Services;
using PS.WPF.DataTemplate;

namespace BranchComparer.Azure;

public class AzureModule : Module
{
    protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
    {
        registration.HandleActivation<IViewResolverService>(ViewResolverServiceActivation);
        registration.HandleActivation<IModelResolverService>(ModelResolverServiceActivation);
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypesWithAttributes(ThisAssembly);
    }

    private void ModelResolverServiceActivation(ILifetimeScope scope, IModelResolverService service)
    {
        service.Collection(VisualRegions.STATUS).Add(scope.Resolve<StatusViewModel>());
    }

    private void ViewResolverServiceActivation(ILifetimeScope scope, IViewResolverService service)
    {
        service.AssociateTemplate<StatusViewModel>(scope.Resolve<IDataTemplate<StatusView>>())
               .AssociateTemplate<SettingsViewModel>(scope.Resolve<IDataTemplate<SettingsView>>());
    }
}
