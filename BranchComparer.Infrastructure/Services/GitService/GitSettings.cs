using Newtonsoft.Json;
using PS;

namespace BranchComparer.Infrastructure.Services.GitService;

public class GitSettings : BaseNotifyPropertyChanged,
                           ICloneable
{
    private TimeSpan? _period;
    private string _repositoryDirectory;
    private bool _showUniqueCommits;

    public GitSettings()
    {
        _showUniqueCommits = true;
    }

    [JsonProperty]
    public TimeSpan? Period
    {
        get { return _period; }
        set { SetField(ref _period, value); }
    }

    [JsonProperty]
    public string RepositoryDirectory
    {
        get { return _repositoryDirectory; }
        set { SetField(ref _repositoryDirectory, value); }
    }

    [JsonProperty]
    public bool ShowUniqueCommits
    {
        get { return _showUniqueCommits; }
        set { SetField(ref _showUniqueCommits, value); }
    }

    public object Clone()
    {
        return new GitSettings
        {
            RepositoryDirectory = RepositoryDirectory,
            Period = Period
        };
    }
}
