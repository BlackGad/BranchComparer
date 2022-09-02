using System.Windows;
using System.Windows.Input;
using BranchComparer.Components.CherryPick;
using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Extensions;
using PS.MVVM.Patterns;
using PS.WPF.Patterns.Command;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<CommitViewModel>))]
public partial class CommitView : IView<CommitViewModel>
{
    public CommitView()
    {
        InitializeComponent();
        this.ForwardVisualLifetimeToViewModel();

        Loaded += OnLoaded;
        BringCherryPickToViewCommand = new RelayUICommand(BringCherryPickToView);
    }

    public IUICommand BringCherryPickToViewCommand { get; }

    public CommitViewModel ViewModel
    {
        get { return DataContext as CommitViewModel; }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CherryPickAdapter.RaiseRegisterEvent(this);
    }

    private void Popup_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void BringCherryPickToView()
    {
        if (ViewModel.IsCherryPickPart)
        {
            CherryPickAdapter.RaiseBringCherryPickToViewEvent(this, ViewModel.CherryPicks.First());
        }
    }
}
