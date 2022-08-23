namespace BranchComparer.Infrastructure.Events;

public record SettingsChangedArgs<TSettings>(TSettings Settings);