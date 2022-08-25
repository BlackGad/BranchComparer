namespace BranchComparer.Infrastructure.Services.AzureService;

public interface IAzureService
{
    AzureItem GetItem(int id);

    void InvalidateSettings();
}
