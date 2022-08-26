using BranchComparer.Infrastructure.Services;
using Newtonsoft.Json;

namespace BranchComparer.Git.Settings;

public class GitSettings : AbstractSettings
{
    private string _repositoryDirectory;

    [JsonProperty]
    public string RepositoryDirectory
    {
        get { return _repositoryDirectory; }
        set { SetField(ref _repositoryDirectory, value); }
    }
}
