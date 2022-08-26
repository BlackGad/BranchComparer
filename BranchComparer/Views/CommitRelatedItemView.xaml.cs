using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Extensions;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<CommitRelatedItemViewModel>))]
public partial class CommitRelatedItemView : IView<CommitRelatedItemViewModel>
{
    public CommitRelatedItemView()
    {
        InitializeComponent();
        this.ForwardVisualLifetimeToViewModel();
    }

    public CommitRelatedItemViewModel ViewModel
    {
        get { return DataContext as CommitRelatedItemViewModel; }
    }
}
