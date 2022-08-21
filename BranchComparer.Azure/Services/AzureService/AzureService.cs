using System.ComponentModel.DataAnnotations;
using BranchComparer.Infrastructure.Services;
using BranchComparer.Infrastructure.Services.Abstract;
using BranchComparer.Infrastructure.Services.Abstract.ServiceSettingsState;
using BranchComparer.Infrastructure.Services.AzureService;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using PS.IoC.Attributes;

namespace BranchComparer.Azure.Services.AzureService;

[DependencyRegisterAsInterface(typeof(IAzureService))]
[DependencyLifetime(DependencyLifetime.InstanceSingle)]
internal class AzureService : AbstractService<AzureSettings>,
                              IAzureService
{
    public AzureService(ISettingsService settingsService)
        : base(settingsService)
    {
    }

    protected override ValidationResult ValidateSettings(AzureSettings settings)
    {
        try
        {
            if (string.IsNullOrEmpty(settings.Project))
            {
                return new ValidationResult("Project not set");
            }

            if (string.IsNullOrEmpty(settings.Secret))
            {
                return new ValidationResult("Secret not set");
            }

            if (string.IsNullOrEmpty(settings.CacheDirectory))
            {
                return new ValidationResult("Cache directory not set");
            }

            SetState(SettingsStateType.Checking, "Connecting to Azure with specified parameters");

            var credentials = new VssBasicCredential(string.Empty, settings.Secret);
            var uri = new Uri("https://dev.azure.com/" + settings.Project);

            using var httpClient = new WorkItemTrackingHttpClient(uri, credentials);

            httpClient.GetFieldAsync(AzureFields.SYSTEM_ID).GetAwaiter().GetResult();

            return null;
        }
        catch (Exception e)
        {
            return new ValidationResult(e.GetBaseException().Message);
        }
    }
}
