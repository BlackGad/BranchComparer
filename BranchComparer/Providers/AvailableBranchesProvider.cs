using System.Windows;
using BranchComparer.Git.Settings;
using BranchComparer.Infrastructure;
using BranchComparer.Infrastructure.Events;
using BranchComparer.Infrastructure.Services.GitService;
using PS.Extensions;
using PS.IoC.Attributes;
using PS.MVVM.Services;
using PS.MVVM.Services.Extensions;
using PS.Threading.ThrottlingTrigger;
using PS.WPF.Extensions;

namespace BranchComparer.Providers;

[DependencyRegisterAsSelf]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
[DependencyAutoActivate]
internal class AvailableBranchesProvider
{
    private readonly IBroadcastService _broadcastService;
    private readonly IGitService _gitService;
    private readonly IModelResolverService _modelResolverService;
    private readonly ThrottlingTrigger _updateModelsTrigger;

    public AvailableBranchesProvider(
        IGitService gitService,
        IModelResolverService modelResolverService,
        IBroadcastService broadcastService)
    {
        _gitService = gitService;
        _modelResolverService = modelResolverService;
        _broadcastService = broadcastService;

        _broadcastService.Subscribe<SettingsChangedArgs<GitSettings>>(OnGitSettingsChanged);

        _updateModelsTrigger = ThrottlingTrigger.Setup()
                                                .Throttle(TimeSpan.FromMilliseconds(100))
                                                .Subscribe<EventArgs>(OnUpdateModelsTriggered)
                                                .Create()
                                                .Activate();
        _updateModelsTrigger.Trigger();
    }

    private void OnUpdateModelsTriggered(object sender, EventArgs e)
    {
        IReadOnlyList<string> availableBranches;
        try
        {
            availableBranches = _gitService.GetAvailableBranches();
        }
        catch
        {
            availableBranches = Array.Empty<string>();
        }

        Application.Current.Dispatcher.Postpone(() =>
        {
            var collection = _modelResolverService.Collection(ModelRegions.AVAILABLE_BRANCHES);
            collection.Clear();
            collection.AddRange(availableBranches);

            var args = new ModelsUpdatedArgs(new[]
            {
                ModelRegions.AVAILABLE_BRANCHES,
            });
            _broadcastService.Broadcast(args);
        });
    }

    private void OnGitSettingsChanged(SettingsChangedArgs<GitSettings> args)
    {
        _updateModelsTrigger.Trigger();
    }
}
