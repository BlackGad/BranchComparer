using System;
using System.Windows;
using System.Windows.Controls;
using PS.WPF.Resources;

namespace BranchComparer.Controls;

public class TileContentControl : ContentControl
{
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius),
                                    typeof(CornerRadius),
                                    typeof(TileContentControl),
                                    new FrameworkPropertyMetadata(default(CornerRadius)));

    static TileContentControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TileContentControl), new FrameworkPropertyMetadata(typeof(TileContentControl)));
        ResourceHelper.SetDefaultStyle(typeof(TileContentControl), Resource.ControlStyle);
    }

    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }

    #region Nested type: Resource

    public static class Resource
    {
        private static readonly Uri Default = new("/BranchComparer;component/Controls/TileContentControl.xaml", UriKind.RelativeOrAbsolute);
        public static readonly ResourceDescriptor ControlStyle = ResourceDescriptor.Create<Style>(Default);
        public static readonly ResourceDescriptor ControlTemplate = ResourceDescriptor.Create<ControlTemplate>(Default);
    }

    #endregion
}
