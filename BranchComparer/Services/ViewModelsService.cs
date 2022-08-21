using System;
using System.Collections.Generic;
using Autofac;
using BranchComparer.Infrastructure.Services.GitService;
using BranchComparer.Infrastructure.Services.ViewModelsService;
using BranchComparer.Infrastructure.ViewModels;
using PS.IoC.Attributes;

namespace BranchComparer.Services;

[DependencyRegisterAsInterface(typeof(IViewModelsService))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
public class ViewModelsService : IViewModelsService
{
    private readonly ILifetimeScope _scope;

    public ViewModelsService(ILifetimeScope scope)
    {
        _scope = scope;
    }

    public IReadOnlyList<CommitViewModel> CreateViewModels(string tag, IEnumerable<Commit> commits)
    {
        //public Commit Commit { get; }

        //public string MergedPR { get; }

        //public string Message { get; }

        //public IReadOnlyList<string> RelatedItems { get; }

        //public string RelatedItemsMessage { get; }

        throw new NotImplementedException();
    }
}
