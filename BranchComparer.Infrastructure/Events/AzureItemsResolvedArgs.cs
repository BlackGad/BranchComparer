using BranchComparer.Infrastructure.Services.AzureService;

namespace BranchComparer.Infrastructure.Events;

public record AzureItemsResolvedArgs(IReadOnlyList<AzureItem> Items);