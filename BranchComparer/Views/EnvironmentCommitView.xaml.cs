using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<EnvironmentCommitViewModel>))]
public partial class EnvironmentCommitView : IView<EnvironmentCommitViewModel>
{
    public EnvironmentCommitView()
    {
        InitializeComponent();
    }

    public EnvironmentCommitViewModel ViewModel
    {
        get { return DataContext as EnvironmentCommitViewModel; }
    }
}
