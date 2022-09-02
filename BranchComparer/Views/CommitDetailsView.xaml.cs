using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<CommitDetailsViewModel>))]
public partial class CommitDetailsView : IView<CommitDetailsViewModel>
{
    public CommitDetailsView()
    {
        InitializeComponent();
    }

    public CommitDetailsViewModel ViewModel
    {
        get { return DataContext as CommitDetailsViewModel; }
    }
}
