using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<EnvironmentCommitPRViewModel>))]
public partial class EnvironmentCommitPRView : IView<EnvironmentCommitPRViewModel>
{
    public EnvironmentCommitPRView()
    {
        InitializeComponent();
    }

    public EnvironmentCommitPRViewModel ViewModel
    {
        get { return DataContext as EnvironmentCommitPRViewModel; }
    }
}
