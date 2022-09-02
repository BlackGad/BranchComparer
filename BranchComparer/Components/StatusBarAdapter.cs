using System.Windows;
using BranchComparer.Infrastructure;
using PS.IoC.Attributes;
using PS.MVVM.Components;
using PS.MVVM.Services;

namespace BranchComparer.Components;

[DependencyRegisterAsSelf]
public class StatusBarAdapter : Adapter<FrameworkElement>
{
    private readonly IModelResolverService _modelResolverService;

    public StatusBarAdapter(IModelResolverService modelResolverService)
    {
        _modelResolverService = modelResolverService;
    }

    public override void Dispose()
    {
    }

    protected override void OnAttach(FrameworkElement container)
    {
        _modelResolverService.Object(VisualRegions.STATUS).Value = container;
    }

    protected override void OnDetach(FrameworkElement container)
    {
        _modelResolverService.Object(VisualRegions.STATUS).Reset();
    }
}
