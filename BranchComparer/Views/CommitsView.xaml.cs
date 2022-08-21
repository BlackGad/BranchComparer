using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<CommitsViewModel>))]
public partial class CommitsView : IView<CommitsViewModel>
{
    public CommitsView()
    {
        InitializeComponent();
    }

    public CommitsViewModel ViewModel
    {
        get { return DataContext as CommitsViewModel; }
    }
}
