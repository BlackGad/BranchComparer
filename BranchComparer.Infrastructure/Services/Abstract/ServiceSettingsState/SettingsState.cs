namespace BranchComparer.Infrastructure.Services.Abstract.ServiceSettingsState;

public class SettingsState : ISettingsState
{
    public string Description { get; set; }

    public SettingsStateType StateType { get; set; }
}
