using System.Windows;
using System.Windows.Controls;
using PS.WPF.Resources;

namespace BranchComparer.Controls.TilesContentControl;

public class TilesContentControlItem : ContentControl
{
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius),
                                    typeof(CornerRadius),
                                    typeof(TilesContentControlItem),
                                    new FrameworkPropertyMetadata(default(CornerRadius)));

    static TilesContentControlItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TilesContentControlItem), new FrameworkPropertyMetadata(typeof(TilesContentControlItem)));
        ResourceHelper.SetDefaultStyle(typeof(TilesContentControlItem), Resource.ControlStyle);
    }

    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }

    #region Nested type: Resource

    public static class Resource
    {
        private static readonly Uri Default = new("/BranchComparer;component/Controls/TilesContentControl/TilesContentControlItem.xaml", UriKind.RelativeOrAbsolute);
        public static readonly ResourceDescriptor ControlStyle = ResourceDescriptor.Create<Style>(Default);
        public static readonly ResourceDescriptor ControlTemplate = ResourceDescriptor.Create<ControlTemplate>(Default);
    }

    #endregion
}
