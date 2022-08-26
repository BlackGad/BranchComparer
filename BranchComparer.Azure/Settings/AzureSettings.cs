using System.IO;
using System.Reflection;
using BranchComparer.Infrastructure.Services;
using Newtonsoft.Json;

namespace BranchComparer.Azure.Settings;

public class AzureSettings : AbstractSettings
{
    private string _cacheDirectory;
    private string _project;
    private string _secret;

    public AzureSettings()
    {
        var assembly = Assembly.GetEntryAssembly();
        var title = assembly?.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "BranchComparer";

        _cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), title);
    }

    [JsonProperty]
    public string CacheDirectory
    {
        get { return _cacheDirectory; }
        set { SetField(ref _cacheDirectory, value); }
    }

    [JsonProperty]
    public string Project
    {
        get { return _project; }
        set { SetField(ref _project, value); }
    }

    [JsonProperty]
    public string Secret
    {
        get { return _secret; }
        set { SetField(ref _secret, value); }
    }
}
