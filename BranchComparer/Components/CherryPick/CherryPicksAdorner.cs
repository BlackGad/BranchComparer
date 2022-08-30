using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using BranchComparer.Infrastructure;
using BranchComparer.ViewModels;
using BranchComparer.Views;
using PS.Extensions;
using PS.IoC.Attributes;
using PS.MVVM.Services;

namespace BranchComparer.Components.CherryPick;

[DependencyRegisterAsSelf]
public class CherryPicksAdorner : Adorner
{
    private readonly IModelResolverService _modelResolverService;

    private readonly List<WeakReference> _views;

    public CherryPicksAdorner(UIElement adornedElement, IModelResolverService modelResolverService)
        : base(adornedElement)
    {
        _modelResolverService = modelResolverService;
        _views = new List<WeakReference>();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var adornedElement = (FrameworkElement)AdornedElement;
        var activeViews = _views
                          .Select(v => v.Target as CommitView)
                          .Where(v => v?.ViewModel is not null)
                          .ToDictionary(v => v.ViewModel, v => v);

        foreach (var cherryPick in _modelResolverService.Collection(Regions.CHERRY_PICKS).Enumerate<CommitCherryPickViewModel>())
        {
            if (!activeViews.TryGetValue(cherryPick.Source, out var resolvedSourceView))
            {
                continue;
            }

            if (!activeViews.TryGetValue(cherryPick.Target, out var resolvedTargetView))
            {
                continue;
            }

            if (!resolvedSourceView.IsVisible || !resolvedTargetView.IsVisible)
            {
                continue;
            }

            var sourceViewAnchorPoint = resolvedSourceView.TranslatePoint(new Point(), adornedElement);
            var targetViewAnchorPoint = resolvedTargetView.TranslatePoint(new Point(), adornedElement);

            var leftView = sourceViewAnchorPoint.X < targetViewAnchorPoint.X ? resolvedSourceView : resolvedTargetView;
            var rightView = sourceViewAnchorPoint.X < targetViewAnchorPoint.X ? resolvedTargetView : resolvedSourceView;

            var sourcePoint = leftView.TranslatePoint(new Point(leftView.ActualWidth, leftView.ActualHeight / 2), adornedElement);
            var targetPoint = rightView.TranslatePoint(new Point(0, leftView.ActualHeight / 2), adornedElement);

            if (sourcePoint.Y < 0 || targetPoint.Y < 0 || sourcePoint.Y > adornedElement.ActualHeight || targetPoint.Y > adornedElement.ActualHeight)
            {
                continue;
            }

            drawingContext.DrawDrawing(new GeometryDrawing());
            drawingContext.DrawLine(new Pen(Brushes.Red, 2), sourcePoint, targetPoint);
        }

        /*if (AdornedElement.GetType() == typeof(ConnectorView))
        {
            ConnectorView target = this.AdornedElement as ConnectorView;
            drawingContext.DrawEllipse(Brushes.Black, null, target.StartPoint, 4, 4);
            drawingContext.DrawEllipse(Brushes.Black, null, target.EndPoint, 4, 4);
        }*/
    }

    public void AddLoadedView(CommitView view)
    {
        var weakReference = new WeakReference(view);

        void OnUnloaded(object sender, RoutedEventArgs args)
        {
            _views.Remove(weakReference);
            view.Unloaded -= OnUnloaded;
        }

        view.Unloaded += OnUnloaded;
        _views.Add(weakReference);

        InvalidateVisual();
    }
}
