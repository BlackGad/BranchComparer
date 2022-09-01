using System;
using System.Windows;
using System.Windows.Controls;
using PS.WPF.Resources;

namespace BranchComparer.Controls.CommitsView;

public class CommitsView : ListBox
{
    public static readonly DependencyProperty AzureItemTemplateSelectorProperty =
        DependencyProperty.Register(nameof(AzureItemTemplateSelector),
                                    typeof(DataTemplateSelector),
                                    typeof(CommitsView),
                                    new FrameworkPropertyMetadata(default(DataTemplateSelector)));

    public static readonly DependencyProperty LayoutFlowDirectionProperty =
        DependencyProperty.Register(nameof(LayoutFlowDirection),
                                    typeof(FlowDirection),
                                    typeof(CommitsView),
                                    new FrameworkPropertyMetadata(default(FlowDirection)));

    public static readonly DependencyProperty MessageTemplateSelectorProperty =
        DependencyProperty.Register(nameof(MessageTemplateSelector),
                                    typeof(DataTemplateSelector),
                                    typeof(CommitsView),
                                    new FrameworkPropertyMetadata(default(DataTemplateSelector)));

    public static readonly DependencyProperty PullRequestTemplateSelectorProperty =
        DependencyProperty.Register(nameof(PullRequestTemplateSelector),
                                    typeof(DataTemplateSelector),
                                    typeof(CommitsView),
                                    new FrameworkPropertyMetadata(default(DataTemplateSelector)));

    static CommitsView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CommitsView), new FrameworkPropertyMetadata(typeof(CommitsView)));
        ResourceHelper.SetDefaultStyle(typeof(CommitsView), Resource.ControlStyle);
    }

    public DataTemplateSelector AzureItemTemplateSelector
    {
        get { return (DataTemplateSelector)GetValue(AzureItemTemplateSelectorProperty); }
        set { SetValue(AzureItemTemplateSelectorProperty, value); }
    }

    public FlowDirection LayoutFlowDirection
    {
        get { return (FlowDirection)GetValue(LayoutFlowDirectionProperty); }
        set { SetValue(LayoutFlowDirectionProperty, value); }
    }

    public DataTemplateSelector MessageTemplateSelector
    {
        get { return (DataTemplateSelector)GetValue(MessageTemplateSelectorProperty); }
        set { SetValue(MessageTemplateSelectorProperty, value); }
    }

    public DataTemplateSelector PullRequestTemplateSelector
    {
        get { return (DataTemplateSelector)GetValue(PullRequestTemplateSelectorProperty); }
        set { SetValue(PullRequestTemplateSelectorProperty, value); }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new CommitsViewItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is CommitsViewItem;
    }

    public void BringIntoViewPublic(object item)
    {
        if (!ItemContainerGenerator.Items.Contains(item))
        {
            return;
        }

        if (GetTemplateChild("PART_Panel") is VirtualizingStackPanel panel)
        {
            panel.BringIndexIntoViewPublic(ItemContainerGenerator.Items.IndexOf(item));
        }
    }

    #region Nested type: Resource

    public static class Resource
    {
        private static readonly Uri Default = new("/BranchComparer;component/Controls/CommitsView/CommitsView.xaml", UriKind.RelativeOrAbsolute);
        public static readonly ResourceDescriptor ControlStyle = ResourceDescriptor.Create<Style>(Default);
        public static readonly ResourceDescriptor ControlTemplate = ResourceDescriptor.Create<ControlTemplate>(Default);
    }

    #endregion
}
