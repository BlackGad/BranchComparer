using System.ComponentModel.DataAnnotations;

namespace BranchComparer.Infrastructure.Services.AzureService;

public enum AzureItemType
{
    [Display(Name = "Unk")]
    Unknown,

    [Display(Name = "PBI")]
    PBI,

    [Display(Name = "Bug")]
    Bug,

    [Display(Name = "Task")]
    Task,

    [Display(Name = "F")]
    Feature,
}
