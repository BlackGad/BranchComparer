using BranchComparer.Infrastructure.Services.GitService;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Git.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<Commit>))]
public partial class CommitView : IView<Commit>
{
    public CommitView()
    {
        InitializeComponent();
    }

    public Commit ViewModel
    {
        get { return DataContext as Commit; }
    }
}
