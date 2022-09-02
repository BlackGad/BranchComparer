using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PS.Extensions;
using PS.WPF.Extensions;
using PS.WPF.Resources;

namespace BranchComparer.Controls.TilesContentControl;

public class TilesContentControl : ItemsControl
{
    public static readonly DependencyProperty ColumnDefinitionsProperty =
        DependencyProperty.Register(nameof(ColumnDefinitions),
                                    typeof(FreezableCollection<ColumnDefinition>),
                                    typeof(TilesContentControl),
                                    new FrameworkPropertyMetadata(default(FreezableCollection<ColumnDefinition>)));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius),
                                    typeof(CornerRadius),
                                    typeof(TilesContentControl),
                                    new FrameworkPropertyMetadata(OnCornerRadiusPropertyChanged));

    public static readonly DependencyProperty LayoutFlowDirectionProperty =
        DependencyProperty.Register(nameof(LayoutFlowDirection),
                                    typeof(FlowDirection),
                                    typeof(TilesContentControl),
                                    new FrameworkPropertyMetadata(OnFlowDirectionPropertyChanged));

    private static void OnCornerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var owner = (TilesContentControl)d;
        owner.UpdateItems();
    }

    private static void OnFlowDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var owner = (TilesContentControl)d;
        owner.UpdatePanel();
        owner.UpdateItems();
    }

    private Grid _mainPanel;

    static TilesContentControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TilesContentControl), new FrameworkPropertyMetadata(typeof(TilesContentControl)));
        ResourceHelper.SetDefaultStyle(typeof(TilesContentControl), Resource.ControlStyle);
    }

    public TilesContentControl()
    {
        ColumnDefinitions = new FreezableCollection<ColumnDefinition>();
        AddHandler(TilesContentControlPanel.PanelLoadedEvent, new RoutedEventHandler(OnMainPanelLoaded));
        AddHandler(TilesContentControlPanel.PanelChildChangedEvent, new RoutedEventHandler(OnMainPanelChildChanged));
    }

    public FreezableCollection<ColumnDefinition> ColumnDefinitions
    {
        get { return (FreezableCollection<ColumnDefinition>)GetValue(ColumnDefinitionsProperty); }
        set { SetValue(ColumnDefinitionsProperty, value); }
    }

    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }

    public FlowDirection LayoutFlowDirection
    {
        get { return (FlowDirection)GetValue(LayoutFlowDirectionProperty); }
        set { SetValue(LayoutFlowDirectionProperty, value); }
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        var result = base.ArrangeOverride(arrangeBounds);
        UpdateItems();
        return result;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TilesContentControlItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TilesContentControlItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        element.SetBindingIfDefault(BorderBrushProperty,
                                    new Binding
                                    {
                                        Source = this,
                                        Path = new PropertyPath(BorderBrushProperty),
                                    });

        Dispatcher.Postpone(UpdateItems);
    }

    private void OnMainPanelChildChanged(object sender, RoutedEventArgs e)
    {
        UpdateItems();
    }

    private void OnMainPanelLoaded(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is Grid grid)
        {
            _mainPanel = grid;

            UpdatePanel();
            UpdateItems();
        }
    }

    private void UpdateItems()
    {
        var itemContainers = ItemContainerGenerator
                             .Items
                             .Enumerate()
                             .Select(item => ItemContainerGenerator.ContainerFromItem(item))
                             .OfType<TilesContentControlItem>()
                             .ToList();

        var firstVisibleContainer = itemContainers.FirstOrDefault(c => c.Visibility != Visibility.Collapsed);
        var lastVisibleContainer = itemContainers.LastOrDefault(c => c.Visibility != Visibility.Collapsed);

        foreach (var itemContainer in itemContainers)
        {
            var containerIndex = ItemContainerGenerator.IndexFromContainer(itemContainer);
            var columnIndex = LayoutFlowDirection == FlowDirection.LeftToRight
                ? containerIndex
                : ItemContainerGenerator.Items.Count - containerIndex - 1;

            Grid.SetColumn(itemContainer, columnIndex);

            var isFirstItem = itemContainer == firstVisibleContainer;
            var isLastItem = itemContainer == lastVisibleContainer;

            itemContainer.CornerRadius = (isFirstItem, isLastItem, LayoutFlowDirection) switch
            {
                { isFirstItem: true, isLastItem: true, } => CornerRadius,
                { isFirstItem: true, LayoutFlowDirection: FlowDirection.LeftToRight, } => new CornerRadius(CornerRadius.TopLeft, 0, 0, CornerRadius.BottomLeft),
                { isFirstItem: true, LayoutFlowDirection: FlowDirection.RightToLeft, } => new CornerRadius(0, CornerRadius.TopLeft, CornerRadius.BottomLeft, 0),
                { isLastItem: true, LayoutFlowDirection: FlowDirection.LeftToRight, } => new CornerRadius(0, CornerRadius.TopRight, CornerRadius.BottomRight, 0),
                { isLastItem: true, LayoutFlowDirection: FlowDirection.RightToLeft, } => new CornerRadius(CornerRadius.TopRight, 0, 0, CornerRadius.BottomRight),
                _ => new CornerRadius(),
            };

            itemContainer.BorderThickness = (isFirstItem, isLastItem, LayoutFlowDirection) switch
            {
                { isFirstItem: true, isLastItem: true, } => BorderThickness,
                { isFirstItem: true, LayoutFlowDirection: FlowDirection.LeftToRight, } => new Thickness(BorderThickness.Left, BorderThickness.Top, 0, BorderThickness.Bottom),
                { isFirstItem: true, LayoutFlowDirection: FlowDirection.RightToLeft, } => new Thickness(0, BorderThickness.Top, BorderThickness.Left, BorderThickness.Bottom),
                { isLastItem: true, LayoutFlowDirection: FlowDirection.LeftToRight, } => new Thickness(0, BorderThickness.Top, BorderThickness.Right, BorderThickness.Bottom),
                { isLastItem: true, LayoutFlowDirection: FlowDirection.RightToLeft, } => new Thickness(BorderThickness.Right, BorderThickness.Top, 0, BorderThickness.Bottom),
                _ => new Thickness(0, BorderThickness.Top, 0, BorderThickness.Bottom),
            };
        }
    }

    private void UpdatePanel()
    {
        if (_mainPanel == null)
        {
            return;
        }

        _mainPanel.ColumnDefinitions.Clear();

        var definitions = LayoutFlowDirection == FlowDirection.LeftToRight
            ? ColumnDefinitions.AsEnumerable()
            : ColumnDefinitions.Reverse();

        _mainPanel.ColumnDefinitions.Clear();
        foreach (var definition in definitions)
        {
            _mainPanel.ColumnDefinitions.Add(definition);
        }
    }

    #region Nested type: Resource

    public static class Resource
    {
        private static readonly Uri Default = new("/BranchComparer;component/Controls/TilesContentControl/TilesContentControl.xaml", UriKind.RelativeOrAbsolute);
        public static readonly ResourceDescriptor ControlStyle = ResourceDescriptor.Create<Style>(Default);
        public static readonly ResourceDescriptor ControlTemplate = ResourceDescriptor.Create<ControlTemplate>(Default);
    }

    #endregion
}
