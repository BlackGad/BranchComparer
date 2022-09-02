using System.Windows;
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
internal class AvailableBranchesProvider : IDisposable
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

        _broadcastService.Subscribe<ServiceStateChangedArgs<IGitService>>(OnGitServiceStateChanged);

        _updateModelsTrigger = ThrottlingTrigger.Setup()
                                                .Throttle(TimeSpan.FromMilliseconds(100))
                                                .Subscribe<EventArgs>(OnUpdateModelsTriggered)
                                                .Create()
                                                .Activate();
        UpdateAvailableBranches();

        _updateModelsTrigger.Trigger();
    }

    public void Dispose()
    {
        _broadcastService.Unsubscribe<ServiceStateChangedArgs<IGitService>>(OnGitServiceStateChanged);
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
            var collection = _modelResolverService.Collection(Regions.AVAILABLE_BRANCHES);
            collection.Clear();
            collection.AddRange(availableBranches);
        });
    }

    private void OnGitServiceStateChanged(ServiceStateChangedArgs<IGitService> args)
    {
        UpdateAvailableBranches();
    }

    private void UpdateAvailableBranches()
    {
        _updateModelsTrigger.Trigger();
    }
}
