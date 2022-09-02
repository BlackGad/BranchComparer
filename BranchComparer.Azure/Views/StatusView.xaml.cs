using System.Windows.Input;
using BranchComparer.Azure.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Azure.Views;

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
