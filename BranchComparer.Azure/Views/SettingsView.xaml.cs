using BranchComparer.Azure.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Azure.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<SettingsViewModel>))]
public partial class SettingsView : IView<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
    }

    public SettingsViewModel ViewModel
    {
        get { return DataContext as SettingsViewModel; }
    }
}
