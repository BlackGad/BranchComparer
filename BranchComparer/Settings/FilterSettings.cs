using BranchComparer.Infrastructure.Services;
using Newtonsoft.Json;

namespace BranchComparer.Settings;

public class FilterSettings : AbstractSettings
{
    private bool _excludeCherryPicks;
    private string _message;
    private TimeSpan? _period;
    private string _release;

    [JsonProperty]
    public bool ExcludeCherryPicks
    {
        get { return _excludeCherryPicks; }
        set { SetField(ref _excludeCherryPicks, value); }
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

   
}
