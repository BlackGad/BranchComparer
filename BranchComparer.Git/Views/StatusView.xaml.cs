using System.Windows.Input;
using BranchComparer.Git.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Git.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<StatusViewModel>))]
public partial class StatusView : IView<StatusViewModel>
{
    public StatusView()
    {
        InitializeComponent();
    }

    public StatusViewModel ViewModel
    {
        get { return DataContext as StatusViewModel; }
    }

    private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}
