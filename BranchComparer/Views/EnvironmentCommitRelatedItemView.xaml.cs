using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Extensions;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<EnvironmentCommitRelatedItemViewModel>))]
public partial class EnvironmentCommitRelatedItemView : IView<EnvironmentCommitRelatedItemViewModel>
{
    public EnvironmentCommitRelatedItemView()
    {
        InitializeComponent();
        this.ForwardVisualLifetimeToViewModel();
    }

    public EnvironmentCommitRelatedItemViewModel ViewModel
    {
        get { return DataContext as EnvironmentCommitRelatedItemViewModel; }
    }
}
