using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using BranchComparer.Infrastructure;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Settings;
using BranchComparer.ViewModels;
using BranchComparer.Views;
using PS.Extensions;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;

namespace BranchComparer.Components.CherryPick;

[DependencyRegisterAsSelf]
public class CherryPicksAdorner : Adorner
{
    public static readonly Brush CherryPickBrush = new SolidColorBrush(Color.FromArgb(100, 230, 0, 0));

    private readonly IModelResolverService _modelResolverService;
    private readonly ISettingsService _settingsService;

    private readonly List<WeakReference> _views;

    public CherryPicksAdorner(
        UIElement adornedElement,
        IModelResolverService modelResolverService,
        ISettingsService settingsService,
        IBroadcastService broadcastService)
        : base(adornedElement)
    {
        _modelResolverService = modelResolverService;
        _settingsService = settingsService;
        _views = new List<WeakReference>();

        broadcastService.Subscribe<SettingsChangedArgs<VisualizationSettings>>(OnVisualizationSettingsChanged);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var settings = _settingsService.GetSettings<VisualizationSettings>();
        if (!settings.IsCherryPickVisible)
        {
            return;
        }

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

            var sourcePoint = Point.Add(leftView.TranslatePoint(new Point(leftView.ActualWidth, leftView.ActualHeight / 2), adornedElement), new Vector(-1, 0));
            var targetPoint = Point.Add(rightView.TranslatePoint(new Point(0, leftView.ActualHeight / 2), adornedElement), new Vector(1, 0));

            if (sourcePoint.Y < 0 || targetPoint.Y < 0 || sourcePoint.Y > adornedElement.ActualHeight || targetPoint.Y > adornedElement.ActualHeight)
            {
                continue;
            }

            drawingContext.DrawDrawing(new GeometryDrawing());
            drawingContext.DrawLine(new Pen(CherryPickBrush, 2), sourcePoint, targetPoint);
        }
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

    private void OnVisualizationSettingsChanged(SettingsChangedArgs<VisualizationSettings> obj)
    {
        InvalidateVisual();
    }
}
