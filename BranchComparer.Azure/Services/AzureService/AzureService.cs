using System.Collections.Concurrent;
using System.Runtime.Caching;
using BranchComparer.Azure.Settings;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.AzureService;
using FluentValidation;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using PS.Extensions;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Runtime.Caching;
using PS.Runtime.Caching.Default;
using PS.Threading;
using PS.Threading.ThrottlingTrigger;

namespace BranchComparer.Azure.Services.AzureService;

[DependencyRegisterAsInterface(typeof(IAzureService))]
[DependencyRegisterAsSelf]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
public class AzureService : IAzureService
{
    private const string HTTPS_DEV_AZURE_COM = "https://dev.azure.com/";

    private static object TryGetField(IDictionary<string, object> fields, string key)
    {
        if (fields.TryGetValue(key, out var value))
        {
            return value;
        }

        return null;
    }

    private static Uri TryGetLink(IReadOnlyDictionary<string, object> links, string key)
    {
        if (links.TryGetValue(key, out var value) && value is ReferenceLink link)
        {
            return new Uri(link.Href);
        }

        return null;
    }

    private readonly ThrottlingTrigger _applySettingsTrigger;
    private readonly IBroadcastService _broadcastService;
    private readonly ThrottlingTrigger _itemsResolveRequiredTrigger;
    private readonly ConcurrentBag<int> _notResolvedItems;

    private readonly ISettingsService _settingsService;
    private readonly IValidator<AzureSettings> _settingsValidator;
    private ObjectCache _cache;
    private DefaultRepository _cacheRepository;

    public AzureService(
        ISettingsService settingsService,
        IBroadcastService broadcastService,
        IValidator<AzureSettings> settingsValidator)
    {
        _settingsService = settingsService;
        _broadcastService = broadcastService;
        _settingsValidator = settingsValidator;
        _notResolvedItems = new ConcurrentBag<int>();
        _itemsResolveRequiredTrigger = ThrottlingTrigger.Setup()
                                                        .Throttle(TimeSpan.FromMilliseconds(100))
                                                        .Subscribe<EventArgs>(OnItemsResolveRequiredTrigger)
                                                        .Create()
                                                        .Activate();
        _applySettingsTrigger = ThrottlingTrigger.Setup()
                                                 .Throttle(TimeSpan.FromMilliseconds(100))
                                                 .Subscribe<EventArgs>(OnApplySettingsTrigger)
                                                 .Create()
                                                 .Activate();

        _broadcastService.Subscribe<SettingsChangedArgs<AzureSettings>>(OnSettingsChanged);

        _applySettingsTrigger.Trigger();
    }

    
    public void ClearCache()
    {
        if (_cache is FileCache fileCache)
        {
            fileCache.Clear();
            _cacheRepository?.Cleanup();
        }

        _applySettingsTrigger.Trigger();
        _broadcastService.Broadcast(new RefreshBranchesArgs());
    }

    public IEnumerable<AzureItem> GetItems(IEnumerable<int> ids)
    {
        var settings = _settingsService.GetSettings<AzureSettings>();
        if (settings == null)
        {
            return null;
        }

        var required = ids.Enumerate().Distinct().ToDictionary(id => id.ToString(), id => id);

        var cachedItems = _cache?.GetValues(required.Keys) ?? new Dictionary<string, object>();
        var missedItems = required.Keys.Except(cachedItems.Keys);
        foreach (var missedItem in missedItems)
        {
            _notResolvedItems.Add(required[missedItem]);
        }

        _itemsResolveRequiredTrigger.Trigger();

        return cachedItems.Values.Enumerate<AzureItem>();
    }

    public void InvalidateSettings()
    {
        _applySettingsTrigger.Trigger();
    }
    
    private void OnApplySettingsTrigger(object sender, EventArgs e)
    {
        var settings = _settingsService.GetSettings<AzureSettings>();
        var validationResult = Async.Run(async () => await _settingsValidator.ValidateAsync(settings));
        
        if (validationResult.IsValid)
        {
            _cacheRepository = new DefaultRepository(settings.CacheDirectory, false);
            _cache = new FileCache(_cacheRepository);
        }

        _itemsResolveRequiredTrigger.Trigger();
    }

    private void OnItemsResolveRequiredTrigger(object sender, EventArgs e)
    {
        var notResolvedItems = new List<int>();
        while (_notResolvedItems.TryTake(out var item))
        {
            notResolvedItems.Add(item);
        }

        notResolvedItems = notResolvedItems.Distinct().ToList();

        if (_cache != null)
        {
            var values = _cache.GetValues(notResolvedItems.Select(i => i.ToString()));
            _broadcastService.Broadcast(new AzureItemsResolvedArgs(values.Values.OfType<AzureItem>().ToList()));
            notResolvedItems = notResolvedItems.Except(values.Keys.Select(int.Parse)).ToList();
        }

        IReadOnlyList<WorkItem> items;
        try
        {
            var settings = _settingsService.GetSettings<AzureSettings>();
            items = Async.Run(async () => await GetWorkItemsAsync(notResolvedItems.ToArray(), settings));
        }
        catch
        {
            foreach (var notResolvedItem in notResolvedItems)
            {
                _notResolvedItems.Add(notResolvedItem);
            }

            return;
        }

        var resolvedItems = new List<AzureItem>();
        foreach (var item in items)
        {
            if (!item.Id.HasValue)
            {
                continue;
            }

            int? parentId = null;
            if (int.TryParse(TryGetField(item.Fields, AzureFields.SYSTEM_PARENT)?.ToString(), out var parsedParentId))
            {
                parentId = parsedParentId;
            }

            var type = TryGetField(item.Fields, AzureFields.SYSTEM_WORK_ITEM_TYPE) switch
            {
                "Product Backlog Item" => AzureItemType.PBI,
                "Task" => AzureItemType.Task,
                "Bug" => AzureItemType.Bug,
                _ => AzureItemType.Unknown,
            };

            var azureItem = new AzureItem
            {
                Id = item.Id.Value,
                State = TryGetField(item.Fields, AzureFields.SYSTEM_STATE)?.ToString(),
                Title = TryGetField(item.Fields, AzureFields.SYSTEM_TITLE)?.ToString(),
                Release = TryGetField(item.Fields, AzureFields.CUSTOM_RELEASE)?.ToString(),
                Hotfix = TryGetField(item.Fields, AzureFields.CUSTOM_HOTFIX)?.ToString(),
                ParentId = parentId,
                Type = type,
                Uri = TryGetLink(item.Links.Links, AzureLinks.HTML),
            };

            _cache.Set(azureItem.Id.ToString(), azureItem, DateTimeOffset.Now + TimeSpan.FromDays(1));
            resolvedItems.Add(azureItem);
        }

        _broadcastService.Broadcast(new AzureItemsResolvedArgs(resolvedItems));
    }

    private async Task<IReadOnlyList<WorkItem>> GetWorkItemsAsync(IReadOnlyList<int> items, AzureSettings settings)
    {
        if (!items.Any())
        {
            return Array.Empty<WorkItem>();
        }

        var uri = new Uri(HTTPS_DEV_AZURE_COM + settings.Project);
        var credentials = new VssBasicCredential(string.Empty, settings.Secret);

        using var httpClient = new WorkItemTrackingHttpClient(uri, credentials);

        var result = new List<WorkItem>();
        var fields = new[]
        {
            AzureFields.SYSTEM_ID,
            AzureFields.SYSTEM_TITLE,
            AzureFields.SYSTEM_STATE,
            AzureFields.CUSTOM_RELEASE,
            AzureFields.CUSTOM_HOTFIX,
            AzureFields.SYSTEM_PARENT,
            AzureFields.SYSTEM_WORK_ITEM_TYPE,
        };

        var workItems = await httpClient.GetWorkItemsAsync(items, expand: WorkItemExpand.Links, fields: fields);
        var taskItems = workItems.Where(i => i.Fields.TryGetValue(AzureFields.SYSTEM_WORK_ITEM_TYPE, out var type) && Equals(type, "Task")).ToList();
        if (taskItems.Any())
        {
            var taskParents = taskItems
                              .Select(i => i.Fields.TryGetValue(AzureFields.SYSTEM_PARENT, out var parent) ? parent : null)
                              .Where(id => id is not null)
                              .Select(Convert.ToInt32);

            foreach (var taskParent in taskParents)
            {
                _notResolvedItems.Add(taskParent);
            }

            _itemsResolveRequiredTrigger.Trigger();
        }

        result.AddRange(workItems);
        
        return result;
    }

    private void OnSettingsChanged(SettingsChangedArgs<AzureSettings> args)
    {
        _applySettingsTrigger.Trigger();
    }
}
