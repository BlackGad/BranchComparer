using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using BranchComparer.Git.ViewModels;
using BranchComparer.Git.Views;
using BranchComparer.Infrastructure;
using PS.IoC.Extensions;
using PS.MVVM.Extensions;
using PS.MVVM.Services;
using PS.WPF.DataTemplate;

namespace BranchComparer.Git;

public class GitModule : Module
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
        service.Collection(Regions.SETTINGS).Add(scope.Resolve<SettingsViewModel>());
    }

    private void ViewResolverServiceActivation(ILifetimeScope scope, IViewResolverService service)
    {
        service.AssociateTemplate<SettingsViewModel>(scope.Resolve<IDataTemplate<SettingsView>>());
    }
}
