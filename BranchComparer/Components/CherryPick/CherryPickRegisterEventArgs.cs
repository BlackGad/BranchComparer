using System.Windows;
using BranchComparer.Views;

namespace BranchComparer.Components.CherryPick;

public sealed class CherryPickRegisterEventArgs : RoutedEventArgs
{
    public CherryPickRegisterEventArgs(RoutedEvent routedEvent, CommitView source)
        : base(routedEvent, source)
    {
    }
}
