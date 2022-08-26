using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<CommitViewModel>))]
public partial class CommitView : IView<CommitViewModel>
{
    public CommitView()
    {
        InitializeComponent();
    }

    public CommitViewModel ViewModel
    {
        get { return DataContext as CommitViewModel; }
    }
}
