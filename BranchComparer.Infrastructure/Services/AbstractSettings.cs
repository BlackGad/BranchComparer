using Newtonsoft.Json;
using PS;

namespace BranchComparer.Infrastructure.Services;

public abstract class AbstractSettings : BaseNotifyPropertyChanged,
                                         ICloneable
{
    public object Clone()
    {
        var serialized = JsonConvert.SerializeObject(this);
        return JsonConvert.DeserializeObject(serialized, GetType()) ?? throw new InvalidOperationException();
    }
}
