using System.Windows;
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
        service.Object(Regions.FILTER).Value = scope.Resolve<FilterViewModel>();

        service.Object(Regions.LEFT_BRANCH).Value = scope.Resolve<CommitsViewModel>(
            TypedParameter.From(Regions.LEFT_BRANCH),
            TypedParameter.From(FlowDirection.LeftToRight));

        service.Object(Regions.RIGHT_BRANCH).Value = scope.Resolve<CommitsViewModel>(
            TypedParameter.From(Regions.RIGHT_BRANCH),
            TypedParameter.From(FlowDirection.RightToLeft));

        service.Collection(Regions.SETTINGS).Add(scope.Resolve<SettingsViewModel>());
    }

    private void ViewResolverServiceActivation(ILifetimeScope scope, IViewResolverService service)
    {
        service.AssociateTemplate<CommitViewModel>(scope.Resolve<IDataTemplate<CommitView>>())
               .AssociateTemplate<CommitsViewModel>(scope.Resolve<IDataTemplate<CommitsView>>())
               .AssociateTemplate<FilterViewModel>(scope.Resolve<IDataTemplate<FilterView>>())
               .AssociateTemplate<SettingsViewModel>(scope.Resolve<IDataTemplate<SettingsView>>());
    }
}
