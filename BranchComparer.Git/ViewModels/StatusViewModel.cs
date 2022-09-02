using System.Windows;
using BranchComparer.Git.Settings;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services;
using FluentValidation;
using PS;
using PS.IoC.Attributes;
using PS.MVVM.Patterns;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Threading.ThrottlingTrigger;
using PS.WPF.Extensions;

namespace BranchComparer.Git.ViewModels;

[DependencyRegisterAsSelf]
public class StatusViewModel : BaseNotifyPropertyChanged,
                               IViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly ThrottlingTrigger _validationRequiredTrigger;
    private readonly IValidator<GitSettings> _validator;
    private bool? _isSettingsValid;
    private string _status;

    public StatusViewModel(
        IBroadcastService broadcastService,
        IValidator<GitSettings> validator,
        ISettingsService settingsService,
        SettingsViewModel settingsViewModel)
    {
        SettingsViewModel = settingsViewModel;
        broadcastService.Subscribe<SettingsChangedArgs<GitSettings>>(OnSettingsChanged);

        _validator = validator;
        _settingsService = settingsService;
        _validationRequiredTrigger = ThrottlingTrigger.Setup()
                                                      .Throttle(TimeSpan.FromMilliseconds(100))
                                                      .Subscribe<EventArgs>(OnValidationRequiredTrigger)
                                                      .Create()
                                                      .Activate();
        _validationRequiredTrigger.Trigger();
    }

    public bool? IsSettingsValid
    {
        get { return _isSettingsValid; }
        set { SetField(ref _isSettingsValid, value); }
    }

    public SettingsViewModel SettingsViewModel { get; }

    public string Status
    {
        get { return _status; }
        set { SetField(ref _status, value); }
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        Application.Current.Dispatcher.Postpone(() => base.OnPropertyChanged(propertyName));
    }

    private async void OnValidationRequiredTrigger(object sender, EventArgs e)
    {
        var settings = _settingsService.GetSettings<GitSettings>();
        var validationResults = await _validator.ValidateAsync(settings);
        if (validationResults.IsValid)
        {
            Status = "Settings are valid";
            IsSettingsValid = true;
        }
        else
        {
            Status = string.Join(Environment.NewLine, validationResults.Errors.Select(error => error.ErrorMessage));
            IsSettingsValid = false;
        }
    }

    private void OnSettingsChanged(SettingsChangedArgs<GitSettings> args)
    {
        _validationRequiredTrigger.Trigger();
    }
}
