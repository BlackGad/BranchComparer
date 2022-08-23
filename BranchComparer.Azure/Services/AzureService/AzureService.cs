using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Caching;
using System.Windows;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.AzureService;
using FluentValidation;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Runtime.Caching.Default;
using PS.Threading;
using PS.WPF.Extensions;

namespace BranchComparer.Azure.Services.AzureService;

[DependencyRegisterAsInterface(typeof(IAzureService))]
[DependencyRegisterAsSelf]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
public class AzureService : BaseNotifyPropertyChanged,
                            IAzureService,
                            IDisposable
{
    private readonly IBroadcastService _broadcastService;
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

    public IReadOnlyList<AzureItem> GetItems(IEnumerable<int> ids)
    {
        return Array.Empty<AzureItem>();
        /*
        var keys = ids.Enumerate().Select(c => c.ToString()).ToList();
        if (State.Status != SettingsStatus.Valid)
        {
            return Array.Empty<AzureItem>();
        }

        var values = _cache.GetValues(keys);
        var missedItems = keys.Except(values.Keys).Select(int.Parse);
        foreach (var missedItem in missedItems)
        {
            _notResolvedItems.Add(missedItem);
        }

        if (_notResolvedItems.Any())
        {
            _itemsResolveRequiredTrigger.Trigger();
        }

        return values.Values.OfType<AzureItem>().ToList();*/
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
        var notResolvedItems = new List<int>();
        while (_notResolvedItems.TryTake(out var item))
        {
            notResolvedItems.Add(item);
        }

        var items = GetWorkItemsAsync(notResolvedItems.Distinct().ToArray()).GetAwaiter().GetResult();
        foreach (var item in items)
        {
            if (!item.Id.HasValue)
            {
                continue;
            }

            int.TryParse(item.Fields[AzureFields.SYSTEM_PARENT]?.ToString(), out var parentId);
            var type = item.Fields[AzureFields.SYSTEM_WORK_ITEM_TYPE] switch
            {
                "Product Backlog Item" => AzureItemType.PBI,
                "Task" => AzureItemType.Task,
                "Bug" => AzureItemType.Bug,
                _ => AzureItemType.Unknown
            };

            var azureItem = new AzureItem
            {
                Id = item.Id.Value,
                State = item.Fields[AzureFields.SYSTEM_STATE]?.ToString(),
                Title = item.Fields[AzureFields.SYSTEM_TITLE]?.ToString(),
                Release = item.Fields[AzureFields.CUSTOM_RELEASE]?.ToString(),
                ParentId = parentId,
                Type = type
            };

            _cache.Set(azureItem.Id.ToString(), azureItem, DateTimeOffset.Now + TimeSpan.FromDays(1));
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
        }
    }

    private AzureSettings GetSettings()
    {
        lock (this)
        {
            if (State.IsValid != true)
            {
                throw new InvalidOperationException($"Settings are not valid. {State.Description}");
            }

            return _settings;
        }
    }

    private async Task<IReadOnlyList<WorkItem>> GetWorkItemsAsync(params int[] items)
    {
        return Array.Empty<WorkItem>();
        /*
        if (!items.Any())
        {
            return Array.Empty<WorkItem>();
        }

        var uri = new Uri("https://dev.azure.com/" + Settings.Project);
        var credentials = new VssBasicCredential(string.Empty, Settings.Secret);

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

            result.AddRange(await GetWorkItemsAsync(taskParents));
        }

        result.AddRange(workItems);

        return result;*/
    }

    private void OnSettingsChanged(SettingsChangedArgs<AzureSettings> args)
    {
        ApplySettings(args.Settings);
        /*try
        {
            SetState(SettingsStatus.Checking, "Validating settings");
            var validationResult = ValidateSettings((TSettings)settings);
            if (validationResult == null)
            {
                SetState(SettingsStatus.Valid, "Settings are valid");
            }
            else
            {
                SetState(SettingsStatus.Invalid, validationResult.ErrorMessage);
            }
        }
        catch (Exception exception)
        {
            SetState(SettingsStatus.Invalid, exception.GetBaseException().Message);
        }*/
    }

    private ValidationResult ValidateSettings(AzureSettings settings)
    {
        return null;
        /*try
        {
            if (string.IsNullOrEmpty(settings.Project))
            {
                return new ValidationResult("Project not set");
            }

            if (string.IsNullOrEmpty(settings.Secret))
            {
                return new ValidationResult("Secret not set");
            }

            if (string.IsNullOrEmpty(settings.CacheDirectory))
            {
                return new ValidationResult("Cache directory not set");
            }

            SetState(SettingsStatus.Checking, "Connecting to Azure with specified parameters");

            var credentials = new VssBasicCredential(string.Empty, settings.Secret);
            var uri = new Uri("https://dev.azure.com/" + settings.Project);

            using var httpClient = new WorkItemTrackingHttpClient(uri, credentials);

            httpClient.GetFieldAsync(AzureFields.SYSTEM_ID).GetAwaiter().GetResult();

            var cleanupSettings = new CleanupSettings //Default settings defined in static CleanupSettings.Default property
            {
                GuarantyFileLifetimePeriod = TimeSpan.FromSeconds(5),
                CleanupPeriod = TimeSpan.FromHours(1)
            };

            _cacheRepository?.Dispose();
            _cacheRepository = new DefaultRepository(settings.CacheDirectory, cleanupSettings: cleanupSettings);
            _cache = new FileCache(_cacheRepository);

            return null;
        }
        catch (Exception e)
        {
            return new ValidationResult(e.GetBaseException().Message);
        }*/
    }
}
