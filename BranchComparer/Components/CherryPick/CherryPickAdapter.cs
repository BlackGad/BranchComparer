using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Autofac;
using BranchComparer.Views;
using PS.IoC.Attributes;
using PS.MVVM.Components;

namespace BranchComparer.Components.CherryPick;

[DependencyRegisterAsSelf]
public class CherryPickAdapter : Adapter<FrameworkElement>
{
    public static readonly RoutedEvent RegisterEvent = EventManager.RegisterRoutedEvent(
        "Register",
        RoutingStrategy.Bubble,
        typeof(CherryPickRegisterEvent),
        typeof(CherryPickAdapter));

    public static void RaiseRegisterEvent(CommitView element)
    {
        element.RaiseEvent(new CherryPickRegisterEventArgs(RegisterEvent, element));
    }

    private readonly ILifetimeScope _scope;
    private CherryPicksAdorner _adorner;

    public CherryPickAdapter(ILifetimeScope scope)
    {
        _scope = scope;
    }

    public override void Dispose()
    {
    }

    protected override void OnAttach(FrameworkElement container)
    {
        container.AddHandler(RegisterEvent, new CherryPickRegisterEvent(OnCherryPickRegister));
        container.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChangedEvent));

        if (AdornerLayer.GetAdornerLayer(container) is AdornerLayer adornerLayer)
        {
            _adorner = _scope.Resolve<CherryPicksAdorner>(TypedParameter.From<UIElement>(container));
            adornerLayer.Add(_adorner);
        }
    }

    protected override void OnDetach(FrameworkElement container)
    {
        container.RemoveHandler(RegisterEvent, new CherryPickRegisterEvent(OnCherryPickRegister));
        container.RemoveHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChangedEvent));

        if (AdornerLayer.GetAdornerLayer(container) is AdornerLayer adornerLayer)
        {
            adornerLayer.Remove(_adorner);
        }
    }

    private void OnCherryPickRegister(object sender, CherryPickRegisterEventArgs e)
    {
        _adorner?.AddLoadedView(e.OriginalSource as CommitView);
    }

    private void OnScrollChangedEvent(object sender, ScrollChangedEventArgs e)
    {
        _adorner?.InvalidateVisual();
    }
}
