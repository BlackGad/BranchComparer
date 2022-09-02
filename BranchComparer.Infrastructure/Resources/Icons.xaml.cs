using System.Windows.Media;
using PS.WPF.Resources;

namespace BranchComparer.Infrastructure.Resources;

public static class Icons
{
    private static readonly Uri Default = new("/BranchComparer.Infrastructure;component/Resources/Icons.xaml", UriKind.RelativeOrAbsolute);
    public static readonly ResourceDescriptor CherryGeometry = ResourceDescriptor.Create<Geometry>(Default);

    public static readonly ResourceDescriptor FileSelectGeometry = ResourceDescriptor.Create<Geometry>(Default);
    public static readonly ResourceDescriptor RefreshGeometry = ResourceDescriptor.Create<Geometry>(Default);
}
