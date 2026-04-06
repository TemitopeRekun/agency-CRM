using System.ComponentModel.DataAnnotations;
using Crm.Domain.Entities;

namespace Crm.Application.DTOs.Clients;

public class CreateClientRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string LegalName { get; set; } = string.Empty;

    [StringLength(50)]
    public string VatNumber { get; set; } = string.Empty;

    [StringLength(500)]
    public string BusinessAddress { get; set; } = string.Empty;

    [StringLength(100)]
    public string Industry { get; set; } = string.Empty;

    public PriorityTier Priority { get; set; }
}

public class ClientResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public PriorityTier Priority { get; set; }
    public DateTime CreatedAt { get; set; }
}
