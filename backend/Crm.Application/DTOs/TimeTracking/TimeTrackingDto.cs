namespace Crm.Application.DTOs.TimeTracking;

public class TimeEntryDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class CreateTimeEntryRequest
{
    public Guid ProjectId { get; set; }
    public Guid TaskId { get; set; }
    public decimal Hours { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
}

public class ProjectTeamResponse
{
    public Guid ProjectId { get; set; }
    public List<ProjectMemberDto> Members { get; set; } = new();
    public decimal TotalHours { get; set; }
    public decimal EstimatedLaborCost { get; set; }
}

public class ProjectMemberDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
}

public class AddTeamMemberRequest
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Contributor";
}
