using System.Windows;
using System.Windows.Data;
using PS.WPF.Resources;

namespace BranchComparer.Infrastructure.Resources;

public static class XamlResources
{
    private static readonly Uri Default = new("/BranchComparer.Infrastructure;component/Resources/XamlResources.xaml", UriKind.RelativeOrAbsolute);

    public static readonly ResourceDescriptor ConfirmationStyle =
        ResourceDescriptor.Create<Style>(description: "Default window style for confirmation view",
                                         resourceDictionary: Default);

    public static readonly ResourceDescriptor NotificationStyle =
        ResourceDescriptor.Create<Style>(description: "Default window style for notification view",
                                         resourceDictionary: Default);

    public static readonly ResourceDescriptor ServiceStateColorConverter = ResourceDescriptor.Create<IValueConverter>(Default);
    public static readonly ResourceDescriptor ServiceStateTitleConverter = ResourceDescriptor.Create<IValueConverter>(Default);
}
