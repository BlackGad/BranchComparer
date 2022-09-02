namespace BranchComparer.Infrastructure.Services.AzureService;

[Serializable]
public class AzureItem
{
    public int Id { get; set; }

    public int? ParentId { get; set; }

    public string Release { get; set; }

    public string State { get; set; }

    public string Title { get; set; }

    public AzureItemType Type { get; set; }

    public Uri Uri { get; set; }
}
