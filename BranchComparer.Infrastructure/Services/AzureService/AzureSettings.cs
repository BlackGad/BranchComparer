using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using PS;

namespace BranchComparer.Infrastructure.Services.AzureService;

public class AzureSettings : BaseNotifyPropertyChanged,
                             ICloneable
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

    object ICloneable.Clone()
    {
        return Clone();
    }

    public AzureSettings Clone()
    {
        return new AzureSettings
        {
            Project = Project,
            Secret = Secret
        };
    }
}
