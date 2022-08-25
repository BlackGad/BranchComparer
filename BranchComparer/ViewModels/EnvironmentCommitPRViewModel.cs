using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class EnvironmentCommitPRViewModel : BaseNotifyPropertyChanged,
                                            IViewModel
{
    public EnvironmentCommitPRViewModel(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
