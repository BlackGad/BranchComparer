using System.Windows;
using BranchComparer.Components.CherryPick;
using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<CommitViewModel>))]
public partial class CommitView : IView<CommitViewModel>
{
    public CommitView()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    public CommitViewModel ViewModel
    {
        get { return DataContext as CommitViewModel; }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CherryPickAdapter.RaiseRegisterEvent(this);
    }
}
