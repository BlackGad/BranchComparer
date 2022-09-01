using System.Windows;
using BranchComparer.ViewModels;

namespace BranchComparer.Components.CherryPick;

public sealed class CherryPickEventArgs : RoutedEventArgs
{
    public CherryPickEventArgs(RoutedEvent routedEvent, object source, CommitCherryPickViewModel cherryPickViewModel)
        : base(routedEvent, source)
    {
        CherryPickViewModel = cherryPickViewModel;
    }

    public CommitCherryPickViewModel CherryPickViewModel { get; }
}
