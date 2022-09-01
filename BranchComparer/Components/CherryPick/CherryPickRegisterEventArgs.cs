using System.Windows;

namespace BranchComparer.Components.CherryPick;

public sealed class CherryPickRegisterEventArgs : RoutedEventArgs
{
    public CherryPickRegisterEventArgs(RoutedEvent routedEvent, object source)
        : base(routedEvent, source)
    {
    }
}
