using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<ShellViewModel>))]
public partial class ShellView : IView<ShellViewModel>
{
    public ShellView()
    {
        InitializeComponent();
    }

    public ShellViewModel ViewModel
    {
        get { return DataContext as ShellViewModel; }
    }
}
