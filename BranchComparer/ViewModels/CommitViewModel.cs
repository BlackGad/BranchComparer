using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Settings;
using PS;
using PS.IoC.Attributes;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class CommitViewModel : BaseNotifyPropertyChanged
{
    private string _author;
    private DateTimeOffset _authorTime;

    private WeakReference<CommitCherryPickViewModel> _cherryPick;
    private DateTimeOffset _committerTime;
    private string _id;
    private string _message;
    private CommitPRViewModel _pr;
    private IReadOnlyList<CommitRelatedItemViewModel> _relatedItems;
    private string _shortMessage;

    public CommitViewModel(ISettingsService settingsService)
    {
        RelatedItems = new ObservableCollection<CommitRelatedItemViewModel>();

        VisualizationSettings = settingsService.GetObservableSettings<VisualizationSettings>();
    }

    public string Author
    {
        get { return _author; }
        set { SetField(ref _author, value); }
    }

    public DateTimeOffset AuthorTime
    {
        get { return _authorTime; }
        set { SetField(ref _authorTime, value); }
    }

    public DateTimeOffset CommitterTime
    {
        get { return _committerTime; }
        set { SetField(ref _committerTime, value); }
    }

    public string Id
    {
        get { return _id; }
        set { SetField(ref _id, value); }
    }

    public bool IsCherryPickPart { get; private set; }

    public string Message
    {
        get { return _message; }
        set { SetField(ref _message, value); }
    }

    public CommitPRViewModel PR
    {
        get { return _pr; }
        set { SetField(ref _pr, value); }
    }

    public IReadOnlyList<CommitRelatedItemViewModel> RelatedItems
    {
        get { return _relatedItems; }
        set { SetField(ref _relatedItems, value); }
    }

    public string ShortMessage
    {
        get { return _shortMessage; }
        set { SetField(ref _shortMessage, value); }
    }

    public VisualizationSettings VisualizationSettings { get; }

    public void AddCherryPickReference(CommitCherryPickViewModel cherryPickViewModel)
    {
        IsCherryPickPart = true;
        _cherryPick = new WeakReference<CommitCherryPickViewModel>(cherryPickViewModel);
    }
}
