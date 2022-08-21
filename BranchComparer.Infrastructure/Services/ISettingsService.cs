namespace BranchComparer.Infrastructure.Services;

public interface ISettingsService
{
    bool Load(string key, object item);

    void LoadPopulateAndSaveOnDispose(string key, object item);

    bool Save(string key, object item);
}
