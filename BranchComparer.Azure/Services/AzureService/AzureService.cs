using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Windows;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.AzureService;
using FluentValidation;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Runtime.Caching;
using PS.Runtime.Caching.Default;
using PS.Threading;
using PS.Threading.ThrottlingTrigger;
using PS.WPF.Extensions;

namespace BranchComparer.Azure.Services.AzureService;

[DependencyRegisterAsInterface(typeof(IAzureService))]
[DependencyRegisterAsSelf]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
public class AzureService : BaseNotifyPropertyChanged,
                            IAzureService,
                            IDisposable
{
    private static object TryGetField(IDictionary<string, object> fields, string key)
    {
        if (fields.TryGetValue(key, out var value))
        {
            return value;
        }

        return null;
    }

    private readonly IBroadcastService _broadcastService;
    private readonly ThrottlingTrigger _itemsResolveRequiredTrigger;
    private readonly ConcurrentBag<int> _notResolvedItems;
    private readonly IValidator<AzureSettings> _settingsValidator;
    private ObjectCache _cache;
    private DefaultRepository _cacheRepository;
    private AzureSettings _settings;
    private ServiceState _state;

    public AzureService(ISettingsService settingsService, IBroadcastService broadcastService, IValidator<AzureSettings> settingsValidator)
    {
        _broadcastService = broadcastService;
        _settingsValidator = settingsValidator;
        _notResolvedItems = new ConcurrentBag<int>();
        _itemsResolveRequiredTrigger = ThrottlingTrigger.Setup()
                                                        .Throttle(TimeSpan.FromMilliseconds(200))
                                                        .Subscribe<EventArgs>(OnItemsResolveRequiredTrigger)
                                                        .Create()
                                                        .Activate();

        _broadcastService.Subscribe<SettingsChangedArgs<AzureSettings>>(OnSettingsChanged);

        ApplySettings(settingsService.GetSettings<AzureSettings>());
    }

    public ServiceState State
    {
        get { return _state; }
        set
        {
            if (SetField(ref _state, value))
            {
                var eventType = typeof(ServiceStateChangedArgs<>).MakeGenericType(typeof(IAzureService));
                var args = Activator.CreateInstance(eventType, _state);
                _broadcastService.Broadcast(eventType, args);
            }

            Application.Current.Dispatcher.Postpone(() => OnPropertyChanged(nameof(State)));
        }
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        Application.Current.Dispatcher.Postpone(() => base.OnPropertyChanged(propertyName));
    }

    public AzureItem GetItem(int id)
    {
        var settings = GetSettings();
        if (settings == null)
        {
            return null;
        }

        var cachedValue = (AzureItem)_cache.Get(id.ToString());
        if (cachedValue == null)
        {
            _notResolvedItems.Add(id);
            _itemsResolveRequiredTrigger.Trigger();
            return null;
        }

        return cachedValue;
    }

    public void InvalidateSettings()
    {
        ApplySettings(_settings);
    }

    public void Dispose()
    {
        _broadcastService.Unsubscribe<SettingsChangedArgs<AzureSettings>>(OnSettingsChanged);
        _cacheRepository?.Dispose();
    }

    private void OnItemsResolveRequiredTrigger(object sender, EventArgs e)
    {
        var settings = GetSettings();
        if (settings == null)
        {
            return;
        }

        var notResolvedItems = new List<int>();
        while (_notResolvedItems.TryTake(out var item))
        {
            notResolvedItems.Add(item);
        }

        IReadOnlyList<WorkItem> items;
        try
        {
            items = Async.Run(async () => await GetWorkItemsAsync(notResolvedItems.Distinct().ToArray(), settings));
        }
        catch
        {
            return;
        }

        foreach (var item in items)
        {
            if (!item.Id.HasValue)
            {
                continue;
            }

            int.TryParse(TryGetField(item.Fields, AzureFields.SYSTEM_PARENT)?.ToString(), out var parentId);
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
                ParentId = parentId,
                Type = type,
            };

            _cache.Set(azureItem.Id.ToString(), azureItem, DateTimeOffset.Now + TimeSpan.FromDays(1));
            _broadcastService.Broadcast(new AzureItemResolvedArgs(azureItem));
        }
    }

    private void ApplySettings(AzureSettings settings)
    {
        var validationResult = Async.Run(async () => await _settingsValidator.ValidateAsync(settings));
        lock (this)
        {
            State = validationResult.IsValid
                ? new ServiceState(true, "Settings are valid")
                : new ServiceState(false, string.Join(Environment.NewLine, validationResult.Errors.Select(e => e.ErrorMessage)));

            _settings = settings;
            if (validationResult.IsValid)
            {
                _cacheRepository = new DefaultRepository(settings.CacheDirectory, false);
                _cache = new FileCache(_cacheRepository);
            }
        }
    }

    private AzureSettings GetSettings()
    {
        lock (this)
        {
            return State.IsValid == true ? _settings : null;
        }
    }

    private async Task<IReadOnlyList<WorkItem>> GetWorkItemsAsync(IReadOnlyList<int> items, AzureSettings settings)
    {
        if (!items.Any())
        {
            return Array.Empty<WorkItem>();
        }

        var uri = new Uri("https://dev.azure.com/" + settings.Project);
        var credentials = new VssBasicCredential(string.Empty, settings.Secret);

        using var httpClient = new WorkItemTrackingHttpClient(uri, credentials);

        var result = new List<WorkItem>();
        var fields = new[]
        {
            AzureFields.SYSTEM_ID,
            AzureFields.SYSTEM_TITLE,
            AzureFields.SYSTEM_STATE,
            AzureFields.CUSTOM_RELEASE,
            AzureFields.SYSTEM_PARENT,
            AzureFields.SYSTEM_WORK_ITEM_TYPE,
        };

        var workItems = await httpClient.GetWorkItemsAsync(items, fields);
        var taskItems = workItems.Where(i => i.Fields.TryGetValue(AzureFields.SYSTEM_WORK_ITEM_TYPE, out var type) && Equals(type, "Task")).ToList();
        if (taskItems.Any())
        {
            var taskParents = taskItems
                              .Select(i => i.Fields.TryGetValue(AzureFields.SYSTEM_PARENT, out var parent) ? parent : null)
                              .Where(id => id is not null)
                              .Select(Convert.ToInt32)
                              .ToArray();

            result.AddRange(await GetWorkItemsAsync(taskParents, settings));
        }

        result.AddRange(workItems);

        return result;
    }

    private void OnSettingsChanged(SettingsChangedArgs<AzureSettings> args)
    {
        ApplySettings(args.Settings);
    }
}
