using System.Windows.Media;
using PS.WPF.Resources;

namespace BranchComparer.Git;

public static class XamlResources
{
    private static readonly Uri Default = new("/BranchComparer.Git;component/XamlResources.xaml", UriKind.RelativeOrAbsolute);

    public static readonly ResourceDescriptor GitGeometry = ResourceDescriptor.Create<Geometry>(Default);
}
