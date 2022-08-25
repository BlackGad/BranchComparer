using System.Windows;
using System.Windows.Controls;

namespace BranchComparer.Controls.TilesContentControl;

public class TilesContentControlPanel : Grid
{
    public static readonly RoutedEvent PanelChildChangedEvent = EventManager.RegisterRoutedEvent(
        "PanelChildChanged",
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TilesContentControlPanel));

    public static readonly RoutedEvent PanelLoadedEvent = EventManager.RegisterRoutedEvent(
        "PanelLoaded",
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TilesContentControlPanel));

    protected override void OnChildDesiredSizeChanged(UIElement child)
    {
        base.OnChildDesiredSizeChanged(child);
        RaisePanelChildChangedEvent();
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);

        RaisePanelLoadedEvent();
    }

    protected virtual void RaisePanelChildChangedEvent()
    {
        RaiseEvent(new RoutedEventArgs(PanelChildChangedEvent, this));
    }

    protected virtual void RaisePanelLoadedEvent()
    {
        RaiseEvent(new RoutedEventArgs(PanelLoadedEvent, this));
    }
}
