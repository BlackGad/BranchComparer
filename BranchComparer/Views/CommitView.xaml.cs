using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using BranchComparer.Components.CherryPick;
using BranchComparer.Controls.CommitsView;
using BranchComparer.ViewModels;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;
using PS.WPF.ValueConverters;

namespace BranchComparer.Views;

[DependencyRegisterAsSelf]
[DependencyRegisterAsInterface(typeof(IView<CommitViewModel>))]
public partial class CommitView : IView<CommitViewModel>
{
    private static MultiBinding CreateCherryPickBinding()
    {
        return new MultiBinding
        {
            Bindings =
            {
                new Binding
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                    {
                        AncestorType = typeof(CommitsView),
                    },
                    Path = new PropertyPath(CommitsView.LayoutFlowDirectionProperty),
                },
                new Binding
                {
                    Path = new PropertyPath(nameof(CommitViewModel.IsCherryPickPart)),
                },
            },
        };
    }

    public CommitView()
    {
        InitializeComponent();

        Loaded += OnLoaded;

        SetupCherryPickElement(LeftCherryPickButton, FlowDirection.RightToLeft);
        SetupCherryPickElement(RightCherryPickButton, FlowDirection.LeftToRight);

        var cherryPickBorderBrushBinding = CreateCherryPickBinding();
        cherryPickBorderBrushBinding.Converter = new RelayMultiValueConverter((objects, _, _, _) =>
        {
            var isPartOfCherryPick = objects.OfType<bool>().FirstOrDefault();
            return isPartOfCherryPick ? CherryPicksAdorner.CherryPickBrush : Brushes.Transparent;
        });

        SetBinding(BackgroundProperty, cherryPickBorderBrushBinding);
        //SetBinding(BorderBrushProperty, cherryPickBorderBrushBinding);
    }

    public CommitViewModel ViewModel
    {
        get { return DataContext as CommitViewModel; }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CherryPickAdapter.RaiseRegisterEvent(this);
    }

    private void SetupCherryPickElement(FrameworkElement element, FlowDirection expectedFlowDirection)
    {
        var visibilityBinding = CreateCherryPickBinding();
        visibilityBinding.Converter = new RelayMultiValueConverter((objects, _, _, _) =>
        {
            var flowDirection = objects.OfType<FlowDirection>().FirstOrDefault();
            if (flowDirection != expectedFlowDirection)
            {
                return Visibility.Collapsed;
            }

            var isPartOfCherryPick = objects.OfType<bool>().FirstOrDefault();
            return isPartOfCherryPick ? Visibility.Visible : Visibility.Hidden;
        });

        element.SetBinding(VisibilityProperty, visibilityBinding);
    }
}
