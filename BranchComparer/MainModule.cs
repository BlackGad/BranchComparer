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
        service.Object(Regions.VISUALIZATION).Value = scope.Resolve<VisualizationViewModel>();
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

        service.AssociateTemplate<FilterViewModel>(scope.Resolve<IDataTemplate<FilterView>>())
               .AssociateTemplate<CommitViewModel>(scope.Resolve<IDataTemplate<CommitView>>())
               .AssociateTemplate<CommitPRViewModel>(scope.Resolve<IDataTemplate<CommitPRView>>())
               .AssociateTemplate<CommitRelatedItemViewModel>(scope.Resolve<IDataTemplate<CommitRelatedItemView>>())
               .AssociateTemplate<VisualizationViewModel>(scope.Resolve<IDataTemplate<VisualizationView>>());
    }
}
