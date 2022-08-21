using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BranchComparer.Infrastructure.Services;
using Newtonsoft.Json;
using NLog;
using PS.IoC.Attributes;

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

    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, object> _saveOnDispose;

    public SettingsService()
    {
        _logger = LogManager.GetCurrentClassLogger();
        _saveOnDispose = new ConcurrentDictionary<string, object>();
    }

    public void Dispose()
    {
        foreach (var pair in _saveOnDispose.ToList())
        {
            Save(pair.Key, pair.Value);
        }
    }

    public bool Load(string key, object item)
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

                return true;
            }
        }
        catch (Exception e)
        {
            _logger.Warn(e, $"Cannot load settings for '{key}'");
        }

        return false;
    }

    public void LoadPopulateAndSaveOnDispose(string key, object item)
    {
        Load(key, item);
        _saveOnDispose.TryAdd(key, item);
    }

    public bool Save(string key, object item)
    {
        try
        {
            var filename = Hash(key);
            var store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            using var stream = new IsolatedStorageFileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, store);
            using var writer = new StreamWriter(stream);

            var json = JsonConvert.SerializeObject(item);
            writer.Write(json);

            return true;
        }
        catch (Exception e)
        {
            _logger.Warn(e, $"Cannot save settings for '{key}'");
        }

        return false;
    }
}
