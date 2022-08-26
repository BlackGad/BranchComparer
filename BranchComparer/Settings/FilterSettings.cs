using System;
using BranchComparer.Infrastructure.Services;
using Newtonsoft.Json;

namespace BranchComparer.Settings;

public class FilterSettings : AbstractSettings
{
    private string _message;
    private TimeSpan? _period;
    private string _release;
    private bool _showUniqueCommits;

    public FilterSettings()
    {
        _showUniqueCommits = true;
    }

    [JsonProperty]
    public string Message
    {
        get { return _message; }
        set { SetField(ref _message, value); }
    }

    [JsonProperty]
    public TimeSpan? Period
    {
        get { return _period; }
        set { SetField(ref _period, value); }
    }

    [JsonProperty]
    public string Release
    {
        get { return _release; }
        set { SetField(ref _release, value); }
    }

    [JsonProperty]
    public bool ShowUniqueCommits
    {
        get { return _showUniqueCommits; }
        set { SetField(ref _showUniqueCommits, value); }
    }
}
