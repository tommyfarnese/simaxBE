using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class ConfirmPoolsRequest
{
    [Required]
    public List<PoolRequest> Pools { get; set; } = new();
}

public class PoolRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100)]
    public int SortOrder { get; set; }

    [Required]
    public List<int> RegistrationIds { get; set; } = new();
}