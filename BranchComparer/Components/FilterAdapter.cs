using System.Windows;
using System.Windows.Controls;
using BranchComparer.Infrastructure.Events;
using PS.IoC.Attributes;
using PS.MVVM.Components;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Patterns.Aware;
using PS.Threading.ThrottlingTrigger;
using PS.WPF.Extensions;

namespace BranchComparer.Components;

[DependencyRegisterAsSelf]
public class FilterAdapter : Adapter<ItemsControl>
{
    private readonly HashSet<ItemsControl> _containers;
    private readonly ThrottlingTrigger _refreshContainerFilterTrigger;

    public FilterAdapter(IBroadcastService broadcastService)
    {
        broadcastService.Subscribe<RequireRefreshBranchFilterViewsArgs>(OnRefreshBranchFilter);
        _containers = new HashSet<ItemsControl>();
        _refreshContainerFilterTrigger = ThrottlingTrigger.Setup()
                                                          .Throttle(TimeSpan.FromMilliseconds(100))
                                                          .Subscribe<EventArgs>(OnRefreshContainerFilter)
                                                          .Create()
                                                          .Activate();
    }

    public override void Dispose()
    {
    }

    protected override void OnAttach(ItemsControl container)
    {
        _containers.Add(container);
        _refreshContainerFilterTrigger.Trigger();
    }

    protected override void OnDetach(ItemsControl container)
    {
        _containers.Remove(container);
    }

    private void OnRefreshContainerFilter(object sender, EventArgs e)
    {
        Application.Current.Dispatcher.Postpone(() =>
        {
            foreach (var container in _containers)
            {
                container.Items.Filter = ItemsFilter;
            }
        });
    }

    private bool ItemsFilter(object item)
    {
        if (item is IIsVisibleAware aware)
        {
            return aware.IsVisible;
        }

        return true;
    }

    private void OnRefreshBranchFilter(RequireRefreshBranchFilterViewsArgs args)
    {
        _refreshContainerFilterTrigger.Trigger();
    }
}
