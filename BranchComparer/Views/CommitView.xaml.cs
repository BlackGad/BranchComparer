using BranchComparer.Infrastructure.Services.GitService;
using PS.IoC.Attributes;
using PS.MVVM.Extensions;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<Commit>))]
public partial class CommitView : IView<Commit>
{
    public CommitView()
    {
        InitializeComponent();

        this.ForwardVisualLifetimeToViewModel();
    }

    public Commit ViewModel
    {
        get { return DataContext as Commit; }
    }
}
