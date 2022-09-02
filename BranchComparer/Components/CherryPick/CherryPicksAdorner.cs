using System.Collections;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using BranchComparer.Infrastructure;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.GitService;
using BranchComparer.Settings;
using BranchComparer.Views;
using PS.Extensions;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.WPF.Extensions;

namespace BranchComparer.Components.CherryPick;

[DependencyRegisterAsSelf]
public class CherryPicksAdorner : Adorner
{
    public static readonly Brush CherryPickBrush = new SolidColorBrush(Color.FromArgb(100, 230, 0, 0));

    private static IEnumerable<IReadOnlyList<object>> GetAllCombinations(IEnumerable<IEnumerable> objects)
    {
        IEnumerable<List<object>> seed = new List<List<object>> { new(), };

        return objects.Aggregate(seed,
                                 (current, agg) =>
                                 {
                                     return current.SelectMany(r => agg.Enumerate()
                                                                       .Select(x => r.Union(new[] { x, }).ToList())
                                                                       .ToList());
                                 });
    }

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
                          .ToLookup(v => v.ViewModel.Commit.Id, v => v);

        foreach (var cherryPick in _modelResolverService.Collection(ModelRegions.CHERRY_PICKS).Enumerate<CommitCherryPick>())
        {
            var resolvedSourceViews = activeViews[cherryPick.SourceId].Where(v => v.IsVisible).ToList();
            var resolvedTargetViews = activeViews[cherryPick.TargetId].Where(v => v.IsVisible).ToList();
            if (resolvedSourceViews.Count == 0 || resolvedTargetViews.Count == 0)
            {
                continue;
            }

            var combinations = GetAllCombinations(new[] { resolvedSourceViews, resolvedTargetViews, });
            foreach (var combination in combinations)
            {
                var resolvedSourceView = (CommitView)combination[0];
                var resolvedTargetView = (CommitView)combination[1];

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
        Application.Current.Dispatcher.Postpone(InvalidateVisual);
    }
}
