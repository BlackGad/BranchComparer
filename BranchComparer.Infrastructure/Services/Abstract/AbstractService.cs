using System.ComponentModel.DataAnnotations;
using System.Windows;
using BranchComparer.Infrastructure.Services.Abstract.ServiceSettingsState;
using PS;
using PS.ComponentModel.DeepTracker;
using PS.Threading.ThrottlingTrigger;
using PS.WPF.Extensions;

namespace BranchComparer.Infrastructure.Services.Abstract;

public abstract class AbstractService<TSettings> : BaseNotifyPropertyChanged,
                                                   IAbstractService<TSettings>,
                                                   IDisposable
    where TSettings : ICloneable, new()
{
    private readonly ThrottlingTrigger _settingsChangedTrigger;
    private readonly DeepTracker _settingsTracker;

    protected AbstractService(ISettingsService settingsService)
    {
        Settings = new TSettings();

        _settingsTracker = DeepTracker.Setup(Settings)
                                      .Subscribe<ChangedPropertyEventArgs>(OnSettingsChanged)
                                      .Create()
                                      .Activate();

        _settingsChangedTrigger = ThrottlingTrigger.Setup()
                                                   .Throttle(TimeSpan.FromMilliseconds(500))
                                                   .Subscribe<EventArgs>(OnSettingsChangedTriggered)
                                                   .Create()
                                                   .Activate();

        _settingsChangedTrigger.Triggered += SettingsChangedOnTrigger;
        settingsService.LoadPopulateAndSaveOnDispose(GetType().AssemblyQualifiedName, Settings);

        InvalidateSettings();
    }

    public TSettings Settings { get; }

    public ISettingsState State { get; private set; }

    public event EventHandler StateChanged;

    public void InvalidateSettings()
    {
        _settingsChangedTrigger.Trigger();
    }

    public void Dispose()
    {
        _settingsChangedTrigger.Dispose();
        _settingsTracker.Dispose();
    }

    private void OnSettingsChanged(object sender, ChangedPropertyEventArgs e)
    {
        _settingsChangedTrigger.Trigger();
    }

    private void OnSettingsChangedTriggered(object sender, EventArgs e)
    {
        var settings = Settings.Clone();

        try
        {
            SetState(SettingsStateType.Checking, "Validating settings");
            var result = ValidateSettings((TSettings)settings);
            if (result == null)
            {
                SetState(SettingsStateType.Valid, "Settings are valid");
            }
            else
            {
                SetState(SettingsStateType.Invalid, result.ErrorMessage);
            }
        }
        catch (Exception exception)
        {
            SetState(SettingsStateType.Invalid, exception.GetBaseException().Message);
        }
    }

    private void SettingsChangedOnTrigger(object sender, EventArgs e)
    {
        SetState(SettingsStateType.Unknown, "Settings were not validated yet");
    }

    protected void SetState(SettingsStateType stateType, string description)
    {
        State = new SettingsState
        {
            StateType = stateType,
            Description = description
        };

        StateChanged?.Invoke(this, EventArgs.Empty);

        Application.Current.Dispatcher.Postpone(() => OnPropertyChanged(nameof(State)));
    }

    protected abstract ValidationResult ValidateSettings(TSettings settings);
}
