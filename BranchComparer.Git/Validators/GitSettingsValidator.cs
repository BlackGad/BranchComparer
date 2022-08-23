using System.IO;
using BranchComparer.Git.Services.GitService;
using FluentValidation;
using LibGit2Sharp;
using PS.IoC.Attributes;

namespace BranchComparer.Git.Validators;

[DependencyRegisterAsInterface(typeof(IValidator<GitSettings>))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class GitSettingsValidator : AbstractValidator<GitSettings>
{
    public GitSettingsValidator()
    {
        RuleFor(settings => settings.RepositoryDirectory)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Repository directory not set")
            .Must(Directory.Exists)
            .WithMessage((_, directory) => $"{directory} not exist")
            .Must(MustBeGitRepository);
    }

    private bool MustBeGitRepository(GitSettings settings, string directory, ValidationContext<GitSettings> context)
    {
        try
        {
            using var repo = new Repository(directory);
            return true;
        }
        catch (Exception e)
        {
            context.AddFailure(e.GetBaseException().Message);
            return false;
        }
    }
}
