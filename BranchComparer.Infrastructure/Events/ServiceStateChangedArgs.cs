using BranchComparer.Infrastructure.Services;

namespace BranchComparer.Infrastructure.Events;

public record ServiceStateChangedArgs<TService>(ServiceState State);
