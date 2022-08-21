namespace BranchComparer.Infrastructure.Services.Abstract.ServiceSettingsState;

public interface ISettingsState
{
    string Description { get; }

    SettingsStateType StateType { get; }
}