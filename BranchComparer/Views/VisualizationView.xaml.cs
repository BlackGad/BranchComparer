using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<VisualizationViewModel>))]
public partial class VisualizationView : IView<VisualizationViewModel>
{
    public VisualizationView()
    {
        InitializeComponent();
    }

    public VisualizationViewModel ViewModel
    {
        get { return DataContext as VisualizationViewModel; }
    }
}
