using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class CommitPRViewModel : BaseNotifyPropertyChanged,
                                            IViewModel
{
    public CommitPRViewModel(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
