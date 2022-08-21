using PS.Patterns.Aware;
using PS.WPF.Controls.BusyContainer;

namespace BranchComparer.Infrastructure.Services;

public interface IBusyService : ITitleAware,
                                IDescriptionAware

{
    bool IsBusy { get; }

    IBusyState Push(string title = null, string description = null);
}
