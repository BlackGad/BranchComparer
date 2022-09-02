using BranchComparer.Infrastructure.Services.GitService;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class CommitDetailsViewModel : BaseNotifyPropertyChanged,
                                      IViewModel
{
    public CommitDetailsViewModel(Commit commit)
    {
        Commit = commit;
    }

    public Commit Commit { get; }
}
