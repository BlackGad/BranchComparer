using BranchComparer.Components;
using BranchComparer.Components.CherryPick;
using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<ShellViewModel>))]
public partial class ShellView : IView<ShellViewModel>
{
    public ShellView(
        CherryPickAdapter cherryPickAdapter,
        FilterAdapter filterAdapter,
        StatusBarAdapter statusBarAdapter)
    {
        CherryPickAdapter = cherryPickAdapter;
        FilterAdapter = filterAdapter;
        StatusBarAdapter = statusBarAdapter;

        InitializeComponent();

        Loaded += (_, _) =>
        {
            CherryPickAdapter.RaiseRegisterEvent(LeftCommits);
            CherryPickAdapter.RaiseRegisterEvent(RightCommits);
        };
    }

    public CherryPickAdapter CherryPickAdapter { get; }

    public FilterAdapter FilterAdapter { get; }

    public StatusBarAdapter StatusBarAdapter { get; }

    public ShellViewModel ViewModel
    {
        get { return DataContext as ShellViewModel; }
    }
}
