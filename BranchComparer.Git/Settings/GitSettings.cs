using BranchComparer.Infrastructure.Services;
using Newtonsoft.Json;

namespace BranchComparer.Git.Settings;

public class GitSettings : AbstractSettings
{
    private string _password;
    private string _repositoryDirectory;
    private bool _showUniqueCommits = true;
    private string _username;

    [JsonProperty]
    public string Password
    {
        get { return _password; }
        set { SetField(ref _password, value); }
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

    [JsonProperty]
    public string Username
    {
        get { return _username; }
        set { SetField(ref _username, value); }
    }
}
