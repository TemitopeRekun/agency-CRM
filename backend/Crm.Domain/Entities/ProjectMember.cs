namespace Crm.Domain.Entities;

public enum ProjectRole
{
    Owner,
    Lead,
    Contributor,
    Viewer
}

public class ProjectMember : BaseEntity, ITenantedEntity
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public ProjectRole Role { get; set; } = ProjectRole.Contributor;
    public Guid TenantId { get; set; }
}
