using System.Windows;
using BranchComparer.Infrastructure.Services.GitService;

namespace BranchComparer.Components.CherryPick;

public sealed class CherryPickEventArgs : RoutedEventArgs
{
    public CherryPickEventArgs(RoutedEvent routedEvent, object source, CommitCherryPick cherryPick)
        : base(routedEvent, source)
    {
        CherryPick = cherryPick;
    }

    public CommitCherryPick CherryPick { get; }
}
