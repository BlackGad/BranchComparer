using BranchComparer.Infrastructure.Services;

namespace BranchComparer.Settings;

public class VisualizationSettings : AbstractSettings
{
    private bool _isCherryPickVisible;
    private bool _isPRVisible;

    public VisualizationSettings()
    {
        _isPRVisible = true;
        _isCherryPickVisible = true;
    }

    public bool IsCherryPickVisible
    {
        get { return _isCherryPickVisible; }
        set { SetField(ref _isCherryPickVisible, value); }
    }

    public bool IsPRVisible
    {
        get { return _isPRVisible; }
        set { SetField(ref _isPRVisible, value); }
    }
}
