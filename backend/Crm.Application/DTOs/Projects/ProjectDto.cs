namespace Crm.Application.DTOs.Projects;

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public Guid OfferId { get; set; }
}

public class ProjectResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Crm.Domain.Entities.ProjectStatus Status { get; set; }
    public Guid? ClientId { get; set; }
    public DateTime CreatedAt { get; set; }
}
