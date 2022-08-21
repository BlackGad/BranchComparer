using System.Windows.Media;
using Autofac;
using BranchComparer.Azure;
using BranchComparer.Git;
using PS.IoC;
using PS.IoC.Extensions;
using PS.MVVM.Services;
using PS.WPF.Theming;

namespace BranchComparer;

public class Bootstrapper : Bootstrapper<ILifetimeScope>
{
    public Bootstrapper(IBootstrapperLogger logger)
        : base(logger)
    {
    }

    protected override void Dispose(IBootstrapperLogger logger)
    {
    }

    protected override void InitializeCriticalComponents(IBootstrapperLogger logger, ILifetimeScope container)
    {
    }

    protected override void DisposeContainer(IBootstrapperLogger logger, ILifetimeScope container)
    {
        logger.Trace("Disposing IOC container");
        container.Dispose();
        logger.Debug("IOC container disposed");
    }

    protected override void ComposeContainer(IBootstrapperLogger logger, ILifetimeScope container)
    {
        logger.Trace("Initializing view resolver...");
        GlobalServices.Register(container.Resolve<IViewResolverService>());
        GlobalServices.Register(container.Resolve<IModelResolverService>());
        logger.Debug("View resolver initialized successfully");
    }

    protected override ILifetimeScope CreateContainer(IBootstrapperLogger logger, ILifetimeScope parentContainer)
    {
        logger.Trace("Configuring IOC builder");
        if (parentContainer == null)
        {
            var builder = new ContainerBuilder();
            RegisterContainerTypes(logger, builder);
            logger.Trace("Building IOC container");
            return builder.Build();
        }

        logger.Trace("Extending parent IOC container with local definitions");
        return parentContainer.BeginDisposableLifetimeScope(builder => RegisterContainerTypes(logger, builder));
    }

    protected override void SetupVisualTheme(IBootstrapperLogger logger, ILifetimeScope container)
    {
        //To load custom font from application resources use:
        //var fontsUri = new Uri("pack://application:,,,/PS.Shell;component/resources/");
        //ThemeFonts.FontFamily = new FontFamily(fontsUri, "./#<Font name>, Courier New");
        Theme.Current.Colors.Accent = Colors.DarkGreen;
    }

    private void RegisterContainerTypes(IBootstrapperLogger logger, ContainerBuilder builder)
    {
        logger.Trace("Registering modules...");

        builder.RegisterModule<MainModule>();
        builder.RegisterModule<AzureModule>();
        builder.RegisterModule<GitModule>();

        logger.Debug("Modules registered");
    }
}
