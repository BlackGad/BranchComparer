using System.Windows;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.WPF.Extensions;

namespace BranchComparer.Services;

[DependencyRegisterAsInterface(typeof(IBroadcastService))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class BroadcastService : PS.MVVM.Services.BroadcastService
{
    protected override void CallDelegate<T>(Delegate @delegate, T args)
    {
        Application.Current.Dispatcher.Postpone(() => @delegate?.DynamicInvoke(args));
    }
}
