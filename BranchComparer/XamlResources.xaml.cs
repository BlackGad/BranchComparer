using System;
using System.Windows;
using PS.WPF.Resources;

namespace BranchComparer;

public static class XamlResources
{
    private static readonly Uri Default = new("/BranchComparer;component/XamlResources.xaml", UriKind.RelativeOrAbsolute);

    public static readonly ResourceDescriptor ShellWindowStyle = ResourceDescriptor.Create<Style>(Default);
}
