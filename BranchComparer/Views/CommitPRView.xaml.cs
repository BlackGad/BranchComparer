using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<CommitPRViewModel>))]
public partial class CommitPRView : IView<CommitPRViewModel>
{
    public CommitPRView()
    {
        InitializeComponent();
    }

    public CommitPRViewModel ViewModel
    {
        get { return DataContext as CommitPRViewModel; }
    }
}
