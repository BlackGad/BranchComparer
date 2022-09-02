using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Autofac;
using BranchComparer.Controls.CommitsView;
using BranchComparer.Infrastructure.Services.GitService;
using BranchComparer.ViewModels;
using BranchComparer.Views;
using PS.Extensions;
using PS.IoC.Attributes;
using PS.MVVM.Components;

namespace BranchComparer.Components.CherryPick;

[DependencyRegisterAsSelf]
public class CherryPickAdapter : Adapter<FrameworkElement>
{
    public static readonly RoutedEvent BringCherryPickToViewEvent = EventManager.RegisterRoutedEvent(
        "BringCherryPickToView",
        RoutingStrategy.Bubble,
        typeof(CherryPickEventHandler),
        typeof(CherryPickAdapter));

    public static readonly RoutedEvent RegisterEvent = EventManager.RegisterRoutedEvent(
        "Register",
        RoutingStrategy.Bubble,
        typeof(CherryPickRegisterEventHandler),
        typeof(CherryPickAdapter));

    public static void RaiseBringCherryPickToViewEvent(UIElement source, CommitCherryPick cherryPick)
    {
        source.RaiseEvent(new CherryPickEventArgs(BringCherryPickToViewEvent, source, cherryPick));
    }

    public static void RaiseRegisterEvent(UIElement element)
    {
        element.RaiseEvent(new CherryPickRegisterEventArgs(RegisterEvent, element));
    }

    private readonly ILifetimeScope _scope;
    private CherryPicksAdorner _adorner;

    private List<WeakReference> _commitsViews;

    public CherryPickAdapter(ILifetimeScope scope)
    {
        _scope = scope;
    }

    public override void Dispose()
    {
    }

    protected override void OnAttach(FrameworkElement container)
    {
        container.AddHandler(RegisterEvent, new CherryPickRegisterEventHandler(OnCherryPickRegister));
        container.AddHandler(BringCherryPickToViewEvent, new CherryPickEventHandler(OnBringCherryPickToView));
        container.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChangedEvent));

        if (AdornerLayer.GetAdornerLayer(container) is AdornerLayer adornerLayer)
        {
            _adorner = _scope.Resolve<CherryPicksAdorner>(TypedParameter.From<UIElement>(container));
            adornerLayer.Add(_adorner);
        }

        _commitsViews = new List<WeakReference>();
    }

    protected override void OnDetach(FrameworkElement container)
    {
        container.RemoveHandler(RegisterEvent, new CherryPickRegisterEventHandler(OnCherryPickRegister));
        container.RemoveHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChangedEvent));
        container.RemoveHandler(BringCherryPickToViewEvent, new CherryPickEventHandler(OnBringCherryPickToView));

        if (AdornerLayer.GetAdornerLayer(container) is AdornerLayer adornerLayer)
        {
            adornerLayer.Remove(_adorner);
        }

        _commitsViews.Clear();
    }

    private void OnBringCherryPickToView(object sender, CherryPickEventArgs e)
    {
        foreach (var view in _commitsViews.Select(c => c.Target).Enumerate<CommitsView>())
        {
            var resolvedSourceCommitViewModel = view.Items.Enumerate<CommitViewModel>().FirstOrDefault(i => i.Commit.Id == e.CherryPick.SourceId);
            var resolvedTargetCommitViewModel = view.Items.Enumerate<CommitViewModel>().FirstOrDefault(i => i.Commit.Id == e.CherryPick.TargetId);
            view.BringIntoViewPublic(resolvedSourceCommitViewModel);
            view.BringIntoViewPublic(resolvedTargetCommitViewModel);
        }
    }

    private void OnCherryPickRegister(object sender, CherryPickRegisterEventArgs e)
    {
        if (e.OriginalSource is CommitView commitView)
        {
            _adorner?.AddLoadedView(commitView);
        }

        if (e.OriginalSource is CommitsView commitsView)
        {
            if (!_commitsViews.Select(c => c.Target).Contains(commitsView))
            {
                _commitsViews.Add(new WeakReference(commitsView));
            }
        }
    }

    private void OnScrollChangedEvent(object sender, ScrollChangedEventArgs e)
    {
        _adorner?.InvalidateVisual();
    }
}
