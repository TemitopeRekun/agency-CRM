using Crm.Application.DTOs.TimeTracking;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Services;

public class TimeTrackingService
{
    private readonly IGenericRepository<TimeEntry> _timeEntryRepository;
    private readonly IGenericRepository<ProjectMember> _projectMemberRepository;
    private readonly IGenericRepository<Project> _projectRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public TimeTrackingService(
        IGenericRepository<TimeEntry> timeEntryRepository,
        IGenericRepository<ProjectMember> projectMemberRepository,
        IGenericRepository<Project> projectRepository,
        IGenericRepository<User> userRepository,
        ICurrentUserContext currentUserContext)
    {
        _timeEntryRepository = timeEntryRepository;
        _projectMemberRepository = projectMemberRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<TimeEntryDto> LogTimeAsync(CreateTimeEntryRequest request)
    {
        var timeEntry = new TimeEntry
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            TaskId = request.TaskId,
            UserId = _currentUserContext.UserId ?? Guid.Empty,
            Hours = request.Hours,
            Description = request.Description,
            Date = request.Date,
            TenantId = _currentUserContext.TenantId ?? Guid.Empty
        };

        await _timeEntryRepository.AddAsync(timeEntry);
        await _timeEntryRepository.SaveChangesAsync();

        return await GetTimeEntryDtoAsync(timeEntry.Id);
    }

    public async Task<IEnumerable<TimeEntryDto>> GetProjectTimeEntriesAsync(Guid projectId)
    {
        var entries = await _timeEntryRepository.AsQueryable()
            .Include(te => te.Project)
            .Include(te => te.Task)
            .Include(te => te.User)
            .Where(te => te.ProjectId == projectId)
            .OrderByDescending(te => te.Date)
            .ToListAsync();

        return entries.Select(MapToDto);
    }

    public async Task<ProjectTeamResponse> GetProjectTeamAsync(Guid projectId)
    {
        var members = await _projectMemberRepository.AsQueryable()
            .Include(pm => pm.User)
            .Where(pm => pm.ProjectId == projectId)
            .ToListAsync();

        var timeEntries = await _timeEntryRepository.AsQueryable()
            .Include(te => te.User)
            .Where(te => te.ProjectId == projectId)
            .ToListAsync();

        var totalHours = timeEntries.Sum(te => te.Hours);
        var laborCost = timeEntries.Sum(te => te.Hours * te.User?.HourlyRate ?? 0);

        return new ProjectTeamResponse
        {
            ProjectId = projectId,
            Members = members.Select(pm => new ProjectMemberDto
            {
                UserId = pm.UserId,
                UserName = pm.User?.FullName ?? "Unknown",
                Email = pm.User?.Email ?? "Unknown",
                Role = pm.Role.ToString(),
                HourlyRate = pm.User?.HourlyRate ?? 0
            }).ToList(),
            TotalHours = totalHours,
            EstimatedLaborCost = laborCost
        };
    }

    public async Task AddTeamMemberAsync(Guid projectId, AddTeamMemberRequest request)
    {
        var existing = await _projectMemberRepository.AsQueryable()
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == request.UserId);
        
        if (existing != null) return;

        var member = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = request.UserId,
            Role = Enum.Parse<ProjectRole>(request.Role),
            TenantId = _currentUserContext.TenantId ?? Guid.Empty
        };

        await _projectMemberRepository.AddAsync(member);
        await _projectMemberRepository.SaveChangesAsync();
    }

    private async Task<TimeEntryDto> GetTimeEntryDtoAsync(Guid id)
    {
        var entry = await _timeEntryRepository.AsQueryable()
            .Include(te => te.Project)
            .Include(te => te.Task)
            .Include(te => te.User)
            .FirstOrDefaultAsync(te => te.Id == id);

        return entry != null ? MapToDto(entry) : new TimeEntryDto();
    }

    private TimeEntryDto MapToDto(TimeEntry te)
    {
        return new TimeEntryDto
        {
            Id = te.Id,
            ProjectId = te.ProjectId,
            ProjectName = te.Project?.Name ?? "Unknown",
            TaskId = te.TaskId,
            TaskTitle = te.Task?.Title ?? "Unknown",
            UserId = te.UserId,
            UserName = te.User?.FullName ?? "Unknown",
            Hours = te.Hours,
            Description = te.Description,
            Date = te.Date
        };
    }
}
