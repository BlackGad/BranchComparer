using System.Windows;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services.AzureService;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;
using PS.MVVM.Patterns.Aware;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.WPF.Extensions;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class CommitRelatedItemViewModel : BaseNotifyPropertyChanged,
                                                     IViewModel,
                                                     ILoadedAware,
                                                     IUnloadedAware
{
    private readonly IAzureService _azureService;
    private readonly IBroadcastService _broadcastService;

    private string _release;

    private string _title;

    private AzureItemType _type;

    public CommitRelatedItemViewModel(int id, IAzureService azureService, IBroadcastService broadcastService)
    {
        _azureService = azureService;
        _broadcastService = broadcastService;
        Id = id;
    }

    public int Id { get; }

    public string Release
    {
        get { return _release; }
        set { SetField(ref _release, value); }
    }

    public string Title
    {
        get { return _title; }
        set { SetField(ref _title, value); }
    }

    public AzureItemType Type
    {
        get { return _type; }
        set { SetField(ref _type, value); }
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        Application.Current.Dispatcher.Postpone(() => base.OnPropertyChanged(propertyName));
    }

    public void Loaded()
    {
        _broadcastService.Subscribe<AzureItemResolvedArgs>(OnAzureItemResolved);

        var resolvedItem = _azureService.GetItem(Id);
        if (resolvedItem != null)
        {
            ApplyResolvedInformation(resolvedItem);
        }
    }

    public void Unloaded()
    {
        _broadcastService.Unsubscribe<AzureItemResolvedArgs>(OnAzureItemResolved);
    }

    private void ApplyResolvedInformation(AzureItem resolvedItem)
    {
        _broadcastService.Unsubscribe<AzureItemResolvedArgs>(OnAzureItemResolved);

        Type = resolvedItem.Type;
        Title = resolvedItem.Title;
        Release = resolvedItem.Release;
    }

    private void OnAzureItemResolved(AzureItemResolvedArgs args)
    {
        if (args.Item.Id != Id)
        {
            return;
        }

        ApplyResolvedInformation(args.Item);
    }
}
