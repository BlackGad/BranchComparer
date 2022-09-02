namespace BranchComparer.Infrastructure.Services.AzureService;

public interface IAzureService
{
    void ClearCache();

    IEnumerable<AzureItem> GetItems(IEnumerable<int> ids);

    void InvalidateSettings();
}
