using BranchComparer.Git.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Git.Views;

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
