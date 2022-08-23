using System.Windows;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using BranchComparer.Infrastructure;
using BranchComparer.Infrastructure.ViewModels;
using BranchComparer.ViewModels;
using BranchComparer.Views;
using PS.IoC.Extensions;
using PS.MVVM.Extensions;
using PS.MVVM.Services;
using PS.WPF.DataTemplate;

namespace BranchComparer;

public class MainModule : Module
{
    protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
    {
        registration.HandleActivation<IViewResolverService>(ViewResolverServiceActivation);
        registration.HandleActivation<IModelResolverService>(ModelResolverServiceActivation);
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypesWithAttributes(ThisAssembly);

        builder.RegisterType<NotificationView>();
        builder.RegisterType<NotificationViewModel>();
        builder.RegisterType<ConfirmationViewModel>();
    }

    private void ModelResolverServiceActivation(ILifetimeScope scope, IModelResolverService service)
    {
        service.Object(Regions.FILTER).Value = scope.Resolve<FilterViewModel>();
        service.Object(Regions.LEFT_BRANCH).Value = scope.Resolve<CommitsViewModel>(TypedParameter.From(FlowDirection.LeftToRight));
        service.Object(Regions.RIGHT_BRANCH).Value = scope.Resolve<CommitsViewModel>(TypedParameter.From(FlowDirection.RightToLeft));
    }

    private void ViewResolverServiceActivation(ILifetimeScope scope, IViewResolverService service)
    {
        service.Associate<ShellViewModel>(
                   template: scope.Resolve<IDataTemplate<ShellView>>(),
                   style: XamlResources.ShellWindowStyle)
               .Associate<NotificationViewModel>(
                   template: scope.Resolve<IDataTemplate<NotificationView>>(),
                   style: Infrastructure.Resources.XamlResources.NotificationStyle)
               .Associate<ConfirmationViewModel>(
                   template: scope.Resolve<IDataTemplate<NotificationView>>(),
                   style: Infrastructure.Resources.XamlResources.ConfirmationStyle);

        service.AssociateTemplate<CommitViewModel>(scope.Resolve<IDataTemplate<CommitView>>())
               .AssociateTemplate<CommitsViewModel>(scope.Resolve<IDataTemplate<CommitsView>>())
               .AssociateTemplate<FilterViewModel>(scope.Resolve<IDataTemplate<FilterView>>());
    }
}
