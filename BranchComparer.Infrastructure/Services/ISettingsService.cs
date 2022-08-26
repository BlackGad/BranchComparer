namespace BranchComparer.Infrastructure.Services;

public interface ISettingsService
{
    T GetObservableSettings<T>()
        where T : AbstractSettings;

    T GetSettings<T>()
        where T : AbstractSettings;

    void LoadPopulateAndSaveOnDispose(string key, object item);
}
