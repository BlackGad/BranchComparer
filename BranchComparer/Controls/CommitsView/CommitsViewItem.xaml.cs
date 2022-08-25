using System;
using System.Windows;
using System.Windows.Controls;
using PS.WPF.Resources;

namespace BranchComparer.Controls.CommitsView;

public class CommitsViewItem : ListBoxItem
{
    static CommitsViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CommitsViewItem), new FrameworkPropertyMetadata(typeof(CommitsViewItem)));
        ResourceHelper.SetDefaultStyle(typeof(CommitsViewItem), Resource.ControlStyle);
    }

    #region Nested type: Resource

    public static class Resource
    {
        private static readonly Uri Default = new("/BranchComparer;component/Controls/CommitsView/CommitsViewItem.xaml", UriKind.RelativeOrAbsolute);
        public static readonly ResourceDescriptor ControlStyle = ResourceDescriptor.Create<Style>(Default);
        public static readonly ResourceDescriptor ControlTemplate = ResourceDescriptor.Create<ControlTemplate>(Default);
    }

    #endregion
}
