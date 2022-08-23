using System.ComponentModel;

namespace BranchComparer.Infrastructure.Services;

public interface ISettingsService
{
    T GetObservableSettings<T>()
        where T : INotifyPropertyChanged, ICloneable;

    T GetSettings<T>()
        where T : INotifyPropertyChanged, ICloneable;

    void LoadPopulateAndSaveOnDispose(string key, object item);
}
