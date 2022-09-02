using System.Windows;
using System.Windows.Controls;
using BranchComparer.Infrastructure.Events;
using PS.IoC.Attributes;
using PS.MVVM.Components;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Patterns.Aware;
using PS.WPF.Extensions;

namespace BranchComparer.Components;

[DependencyRegisterAsSelf]
public class FilterAdapter : Adapter<ItemsControl>
{
    private readonly HashSet<ItemsControl> _containers;

    public FilterAdapter(IBroadcastService broadcastService)
    {
        broadcastService.Subscribe<RefreshBranchFilterViewsArgs>(OnRefreshBranchFilter);
        _containers = new HashSet<ItemsControl>();
    }

    public override void Dispose()
    {
    }

    protected override void OnAttach(ItemsControl container)
    {
        _containers.Add(container);

        RefreshContainerFilter();
    }

    protected override void OnDetach(ItemsControl container)
    {
        _containers.Remove(container);
    }

    private bool ItemsFilter(object item)
    {
        if (item is IIsVisibleAware aware)
        {
            return aware.IsVisible;
        }

        return true;
    }

    private void OnRefreshBranchFilter(RefreshBranchFilterViewsArgs args)
    {
        RefreshContainerFilter();
    }

    private void RefreshContainerFilter()
    {
        Application.Current.Dispatcher.SafeCall(() =>
        {
            foreach (var container in _containers)
            {
                container.Items.Filter = ItemsFilter;
            }
        });
    }
}
