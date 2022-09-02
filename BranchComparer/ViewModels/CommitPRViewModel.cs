using System.Diagnostics;
using BranchComparer.Infrastructure;
using BranchComparer.Infrastructure.Services.GitService;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;
using PS.WPF.Patterns.Command;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class CommitPRViewModel : BaseNotifyPropertyChanged,
                                 IViewModel
{
    private readonly IGitService _gitService;

    public CommitPRViewModel(int id, IGitService gitService)
    {
        _gitService = gitService;
        Id = id;
        NavigateCommand = new RelayUICommand(Navigate);
    }

    public int Id { get; }

    public IUICommand NavigateCommand { get; }

    private void Navigate()
    {
        try
        {
            var uri = _gitService.GetPRItemUri(Id);
            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = uri.AbsoluteUri;
            process.Start();
        }
        catch (Exception e)
        {
            throw new NotificationException("Cannot navigate to PR", e);
        }
    }
}
