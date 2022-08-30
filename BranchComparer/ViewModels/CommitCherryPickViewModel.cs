using PS;
using PS.IoC.Attributes;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class CommitCherryPickViewModel : BaseNotifyPropertyChanged
{
    private CommitViewModel _source;
    private CommitViewModel _target;

    public CommitViewModel Source
    {
        get { return _source; }
        set { SetField(ref _source, value); }
    }

    public CommitViewModel Target
    {
        get { return _target; }
        set { SetField(ref _target, value); }
    }
}
