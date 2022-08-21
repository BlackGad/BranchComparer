using BranchComparer.Infrastructure.Services.Abstract;

namespace BranchComparer.Infrastructure.Services.AzureService;

public interface IAzureService : IAbstractService<AzureSettings>
{
}

public record AzureItem
{
    public string Id { get; init; }

    public AzureItem Parent { get; init; }

    public string Release { get; init; }

    public string State { get; init; }

    public string Title { get; init; }

    public AzureItemType Type { get; init; }
}

public enum AzureItemType
{
    PBI,
    Bug,
    Task,
    Feature
}
