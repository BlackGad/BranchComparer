using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using Newtonsoft.Json;
using NLog;
using PS.ComponentModel.DeepTracker;
using PS.ComponentModel.Navigation;
using PS.ComponentModel.Navigation.Extensions;
using PS.IoC.Attributes;
using PS.MVVM.Services;

namespace BranchComparer.Services;

[DependencyRegisterAsInterface(typeof(ISettingsService))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class SettingsService : ISettingsService,
                                 IDisposable
{
    private static string Hash(string source)
    {
        source ??= "45E4F285-99B2-41F4-8923-A7722DAB7DE6";
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(source);
        var hashBytes = md5.ComputeHash(inputBytes);
        var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        return hash;
    }

    private readonly IBroadcastService _broadcastService;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, object> _saveOnDispose;
    private readonly ObservableCollection<object> _settings;
    private readonly DeepTracker _settingsTracker;

    public SettingsService(IBroadcastService broadcastService)
    {
        _broadcastService = broadcastService;
        _logger = LogManager.GetCurrentClassLogger();
        _saveOnDispose = new ConcurrentDictionary<string, object>();
        _settings = new ObservableCollection<object>();

        _settingsTracker = DeepTracker.Setup(_settings)
                                      .Subscribe<ChangedPropertyEventArgs>(OnSettingsChanged)
                                      .Create()
                                      .Activate();
    }

    void IDisposable.Dispose()
    {
        _settingsTracker.Dispose();

        foreach (var pair in _saveOnDispose.ToList())
        {
            Save(pair.Key, pair.Value);
        }
    }

    public T GetObservableSettings<T>()
        where T : INotifyPropertyChanged, ICloneable
    {
        lock (_settings)
        {
            var existing = _settings.OfType<T>().FirstOrDefault();
            if (existing == null)
            {
                existing = Activator.CreateInstance<T>();
                LoadPopulateAndSaveOnDispose(typeof(T).AssemblyQualifiedName, existing);
                _settings.Add(existing);
            }

            return existing;
        }
    }

    public T GetSettings<T>()
        where T : INotifyPropertyChanged, ICloneable
    {
        return (T)GetObservableSettings<T>().Clone();
    }

    public void LoadPopulateAndSaveOnDispose(string key, object item)
    {
        Load(key, item);
        _saveOnDispose.TryAdd(key, item);
    }

    private void OnSettingsChanged(object sender, ChangedPropertyEventArgs e)
    {
        var owner = (DeepTracker)sender;
        if (owner.GetObject(e.Route.Select(Routes.Wildcard)) is not ICloneable root)
        {
            return;
        }

        var eventType = typeof(SettingsChangedArgs<>).MakeGenericType(root.GetType());
        var args = Activator.CreateInstance(eventType, root.Clone());
        _broadcastService.Broadcast(eventType, args);
    }

    private void Load(string key, object item)
    {
        try
        {
            var filename = Hash(key);
            var store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            if (store.FileExists(filename))
            {
                using var stream = new IsolatedStorageFileStream(filename, FileMode.Open, FileAccess.Read, store);
                using var reader = new StreamReader(stream);

                var json = reader.ReadToEnd();
                JsonConvert.PopulateObject(json, item);
            }
        }
        catch (Exception e)
        {
            _logger.Warn(e, $"Cannot load settings for '{key}'");
        }
    }

    private void Save(string key, object item)
    {
        try
        {
            var filename = Hash(key);
            var store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            using var stream = new IsolatedStorageFileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, store);
            using var writer = new StreamWriter(stream);

            var json = JsonConvert.SerializeObject(item);
            writer.Write(json);
        }
        catch (Exception e)
        {
            _logger.Warn(e, $"Cannot save settings for '{key}'");
        }
    }
}
