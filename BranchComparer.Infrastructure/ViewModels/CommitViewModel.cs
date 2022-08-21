using BranchComparer.Infrastructure.Services.GitService;
using PS;

namespace BranchComparer.Infrastructure.ViewModels;

public class CommitViewModel : BaseNotifyPropertyChanged
{
    private object _messageViewModel;
    private object _pullRequestViewModel;
    private object _relatedItemsViewModel;

    public CommitViewModel(Commit commit)
    {
        Commit = commit;
    }

    public Commit Commit { get; }

    public object MessageViewModel
    {
        get { return _messageViewModel; }
        set { SetField(ref _messageViewModel, value); }
    }

    public object PullRequestViewModel
    {
        get { return _pullRequestViewModel; }
        set { SetField(ref _pullRequestViewModel, value); }
    }

    public object RelatedItemsViewModel
    {
        get { return _relatedItemsViewModel; }
        set { SetField(ref _relatedItemsViewModel, value); }
    }
}
