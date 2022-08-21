using System.Windows;
using System.Windows.Controls;
using PS.WPF.Resources;

namespace BranchComparer.Infrastructure.Components;

public class LogControl : ItemsControl
{
    static LogControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LogControl), new FrameworkPropertyMetadata(typeof(LogControl)));
        ResourceHelper.SetDefaultStyle(typeof(LogControl), Resource.ControlStyle);
    }

    #region Nested type: Resource

    public static class Resource
    {
        private static readonly Uri Default = new("/BranchComparer.Infrastructure;component/Components/LogControl.xaml", UriKind.RelativeOrAbsolute);
        public static readonly ResourceDescriptor ControlStyle = ResourceDescriptor.Create<Style>(Default);
        public static readonly ResourceDescriptor ControlTemplate = ResourceDescriptor.Create<ControlTemplate>(Default);

        public static readonly ResourceDescriptor ItemTemplate = ResourceDescriptor.Create<DataTemplate>(Default);
        public static readonly ResourceDescriptor PanelTemplate = ResourceDescriptor.Create<ItemsPanelTemplate>(Default);
    }

    #endregion
}
