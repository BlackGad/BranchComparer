using System.Windows;
using System.Windows.Data;
using PS.WPF.Resources;

namespace BranchComparer.Infrastructure.Resources;

public static class XamlResources
{
    private static readonly Uri Default = new("/BranchComparer.Infrastructure;component/Resources/XamlResources.xaml", UriKind.RelativeOrAbsolute);

    public static readonly ResourceDescriptor ConfirmationStyle = ResourceDescriptor.Create<Style>(Default);
    public static readonly ResourceDescriptor NotificationStyle = ResourceDescriptor.Create<Style>(Default);
    public static readonly ResourceDescriptor SettingsValidationStateColorConverter = ResourceDescriptor.Create<IValueConverter>(Default);
}
