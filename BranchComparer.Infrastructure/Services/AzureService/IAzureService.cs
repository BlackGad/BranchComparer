namespace BranchComparer.Infrastructure.Services.AzureService;

public interface IAzureService
{
    IReadOnlyList<AzureItem> GetItems(IEnumerable<int> ids);

    void InvalidateSettings();
}
