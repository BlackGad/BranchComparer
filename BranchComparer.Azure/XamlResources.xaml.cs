using System.Windows.Media;
using PS.WPF.Resources;

namespace BranchComparer.Azure;

public static class XamlResources
{
    private static readonly Uri Default = new("/BranchComparer.Azure;component/XamlResources.xaml", UriKind.RelativeOrAbsolute);

    public static readonly ResourceDescriptor AzureGeometry = ResourceDescriptor.Create<Geometry>(Default);
}
