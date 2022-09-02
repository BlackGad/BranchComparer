using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using BranchComparer.Infrastructure;
using BranchComparer.Infrastructure.Services.AzureService;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;
using PS.WPF.Extensions;
using PS.WPF.Patterns.Command;

namespace BranchComparer.ViewModels;

[DependencyRegisterAsSelf]
public class CommitRelatedItemViewModel : BaseNotifyPropertyChanged,
                                          IViewModel
{
    private string _hotfix;
    private int? _parentId;
    private Version _release;
    private string _title;
    private AzureItemType _type;
    private Uri _uri;

    public CommitRelatedItemViewModel(int id)
    {
        Id = id;
        NavigateCommand = new RelayUICommand(Navigate);
    }

    public string Hotfix
    {
        get { return _hotfix; }
        set { SetField(ref _hotfix, value); }
    }

    public int Id { get; }

    public IUICommand NavigateCommand { get; }

    public int? ParentId
    {
        get { return _parentId; }
        set { SetField(ref _parentId, value); }
    }

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

    public void ApplyResolvedInformation(AzureItem resolvedItem)
    {
        Type = resolvedItem.Type;
        Title = resolvedItem.Title;
        Uri = resolvedItem.Uri;
        ParentId = resolvedItem.ParentId;

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

        Hotfix = resolvedItem.Hotfix;
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
}
