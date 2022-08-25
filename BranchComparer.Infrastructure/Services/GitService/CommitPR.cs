using PS;

namespace BranchComparer.Infrastructure.Services.GitService;

public class CommitPR : BaseNotifyPropertyChanged
{
    private int _id;

    public int Id
    {
        get { return _id; }
        set { SetField(ref _id, value); }
    }
}
