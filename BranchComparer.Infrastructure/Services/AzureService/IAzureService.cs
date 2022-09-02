namespace BranchComparer.Infrastructure.Services.AzureService;

public interface IAzureService
{
    IEnumerable<AzureItem> GetItems(IEnumerable<int> ids);

    void InvalidateSettings();
}
