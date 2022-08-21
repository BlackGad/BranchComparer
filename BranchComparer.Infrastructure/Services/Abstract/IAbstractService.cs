using BranchComparer.Infrastructure.Services.Abstract.ServiceSettingsState;

namespace BranchComparer.Infrastructure.Services.Abstract;

public interface IAbstractService<out TSettings>
{
    TSettings Settings { get; }

    ISettingsState State { get; }

    event EventHandler StateChanged;

    void InvalidateSettings();
}
