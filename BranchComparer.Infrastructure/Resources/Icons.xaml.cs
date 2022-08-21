using System.Windows.Media;
using PS.WPF.Resources;

namespace BranchComparer.Infrastructure.Resources;

public static class Icons
{
    private static readonly Uri Default = new("/BranchComparer.Infrastructure;component/Resources/Icons.xaml", UriKind.RelativeOrAbsolute);

    public static readonly ResourceDescriptor FileGeometry =
        ResourceDescriptor.Create<Geometry>(description: "File geometry",
                                            resourceDictionary: Default);

    public static readonly ResourceDescriptor FileSelectGeometry =
        ResourceDescriptor.Create<Geometry>(description: "File select geometry",
                                            resourceDictionary: Default);
}
