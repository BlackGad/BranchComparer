using System.IO;
using BranchComparer.Git.Settings;
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
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(settings => settings.RepositoryDirectory)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Repository directory not set")
            .Must(Directory.Exists)
            .WithMessage((_, directory) => $"{directory} not exist");

        RuleFor(settings => settings.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Credentials are not set");

        RuleFor(settings => settings)
            .Must(MustBeGitRepository)
            .WithMessage("Repository check failed");
    }

    private bool MustBeGitRepository(GitSettings settings, GitSettings _, ValidationContext<GitSettings> context)
    {
        try
        {
            using var repo = new Repository(settings.RepositoryDirectory);
            var remote = repo.Network.Remotes.FirstOrDefault();
            if (remote == null)
            {
                throw new Exception("Configured repository has not connected to remote");
            }

            try
            {
                Repository.ListRemoteReferences(remote.Url,
                                                (_, _, _) => new UsernamePasswordCredentials
                                                {
                                                    Username = settings.Username ?? string.Empty,
                                                    Password = settings.Password ?? string.Empty,
                                                });
            }
            catch (LibGit2SharpException e)
            {
                throw new Exception($"Login failed. Details: {e.GetBaseException().Message}");
            }

            return true;
        }
        catch (Exception e)
        {
            context.AddFailure(e.GetBaseException().Message);
            return false;
        }
    }
}
