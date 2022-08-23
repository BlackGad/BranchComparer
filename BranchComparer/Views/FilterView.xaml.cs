using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<FilterViewModel>))]
public partial class FilterView : IView<FilterViewModel>
{
    public FilterView()
    {
        InitializeComponent();
    }

    public FilterViewModel ViewModel
    {
        get { return DataContext as FilterViewModel; }
    }
}
