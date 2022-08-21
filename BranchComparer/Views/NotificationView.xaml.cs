using BranchComparer.Infrastructure.ViewModels;
using PS.MVVM.Patterns;

namespace BranchComparer.Views;

public partial class NotificationView : IView<NotificationViewModel>
{
    public NotificationView()
    {
        InitializeComponent();
    }

    public NotificationViewModel ViewModel
    {
        get { return DataContext as NotificationViewModel; }
    }
}
