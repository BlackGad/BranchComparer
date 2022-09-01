using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using BranchComparer.Infrastructure;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services.AzureService;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;
using PS.MVVM.Patterns.Aware;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.WPF.Extensions;
using PS.WPF.Patterns.Command;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class CommitRelatedItemViewModel : BaseNotifyPropertyChanged,
                                          IViewModel,
                                          ILoadedAware,
                                          IUnloadedAware
{
    private readonly IAzureService _azureService;
    private readonly IBroadcastService _broadcastService;

    private Version _release;

    private string _title;

    private AzureItemType _type;

    private Uri _uri;

    public CommitRelatedItemViewModel(int id, IAzureService azureService, IBroadcastService broadcastService)
    {
        _azureService = azureService;
        _broadcastService = broadcastService;
        Id = id;
        NavigateCommand = new RelayUICommand(Navigate);
    }

    public int Id { get; }

    public IUICommand NavigateCommand { get; }

    public Version Release
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

    public Uri Uri
    {
        get { return _uri; }
        set { SetField(ref _uri, value); }
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
        Uri = resolvedItem.Uri;

        var versionParts = Regex.Matches(resolvedItem.Release ?? string.Empty, @"(\d+)\.?")
                                .Where(m => m.Success)
                                .Select(m => int.Parse(m.Groups[1].Value))
                                .ToList();

        Release = versionParts switch
        {
            { Count: 1, } => new Version(versionParts[0], 0),
            { Count: 2, } => new Version(versionParts[0], versionParts[1]),
            { Count: 3, } => new Version(versionParts[0], versionParts[1], versionParts[2]),
            { Count: 4, } => new Version(versionParts[0], versionParts[1], versionParts[2], versionParts[3]),
            _ => null,
        };
    }

    private void Navigate()
    {
        try
        {
            if (Uri == null)
            {
                throw new InvalidOperationException("Item not resolved");
            }

            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = Uri.AbsoluteUri;
            process.Start();
        }
        catch (Exception e)
        {
            throw new NotificationException("Cannot navigate to Azure item", e);
        }
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
