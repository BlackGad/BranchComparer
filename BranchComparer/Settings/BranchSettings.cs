using BranchComparer.Infrastructure.Services;
using Newtonsoft.Json;

namespace BranchComparer.Settings;

public class BranchSettings : AbstractSettings
{
    private string _left;
    private string _right;

    [JsonProperty]
    public string Left
    {
        get { return _left; }
        set { SetField(ref _left, value); }
    }

    [JsonProperty]
    public string Right
    {
        get { return _right; }
        set { SetField(ref _right, value); }
    }
}
