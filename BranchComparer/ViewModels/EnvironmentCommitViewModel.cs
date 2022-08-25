using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BranchComparer.Infrastructure.Services.EnvironmentService;
using PS;

namespace BranchComparer.ViewModels;

public class EnvironmentCommitViewModel : BaseNotifyPropertyChanged,
                                          IEnvironmentCommit
{
    private string _author;
    private string _cherryPickSource;
    private string _cherryPickTarget;
    private string _id;
    private string _message;
    private object _pR;
    private IReadOnlyList<object> _relatedItems;
    private string _shortMessage;
    private DateTimeOffset _time;

    public EnvironmentCommitViewModel()
    {
        RelatedItems = new ObservableCollection<object>();
    }

    public string Author
    {
        get { return _author; }
        set { SetField(ref _author, value); }
    }

    public string CherryPickSource
    {
        get { return _cherryPickSource; }
        set { SetField(ref _cherryPickSource, value); }
    }

    public string CherryPickTarget
    {
        get { return _cherryPickTarget; }
        set { SetField(ref _cherryPickTarget, value); }
    }

    public string Id
    {
        get { return _id; }
        set { SetField(ref _id, value); }
    }

    public string Message
    {
        get { return _message; }
        set { SetField(ref _message, value); }
    }

    public object PR
    {
        get { return _pR; }
        set { SetField(ref _pR, value); }
    }

    public IReadOnlyList<object> RelatedItems
    {
        get { return _relatedItems; }
        set { SetField(ref _relatedItems, value); }
    }

    public string ShortMessage
    {
        get { return _shortMessage; }
        set { SetField(ref _shortMessage, value); }
    }

    public DateTimeOffset Time
    {
        get { return _time; }
        set { SetField(ref _time, value); }
    }
}
