using BranchComparer.Azure.Services.AzureService;
using FluentValidation;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using PS.IoC.Attributes;

namespace BranchComparer.Azure.Validators;

[DependencyRegisterAsInterface(typeof(IValidator<AzureSettings>))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class AzureSettingsValidator : AbstractValidator<AzureSettings>
{
    public AzureSettingsValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(settings => settings.Project)
            .NotEmpty()
            .WithMessage("Project not set");

        RuleFor(settings => settings.Secret)
            .NotEmpty()
            .WithMessage("Secret not set");

        RuleFor(settings => settings.CacheDirectory)
            .NotEmpty()
            .WithMessage("Cache directory not set");

        RuleFor(settings => settings)
            .MustAsync(ConnectToAzureAsync);
    }

    private async Task<bool> ConnectToAzureAsync(AzureSettings settings, AzureSettings setting, ValidationContext<AzureSettings> context, CancellationToken token)
    {
        try
        {
            var credentials = new VssBasicCredential(string.Empty, settings.Secret);
            var uri = new Uri("https://dev.azure.com/" + settings.Project);

            using var httpClient = new WorkItemTrackingHttpClient(uri, credentials);

            await httpClient.GetFieldAsync(AzureFields.SYSTEM_ID, cancellationToken: token);
            return true;
        }
        catch (Exception e)
        {
            context.AddFailure(e.GetBaseException().Message);
            return false;
        }
    }
}
