using Crm.Application.DTOs.Tasks;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;

namespace Crm.Application.Services;

public class TaskService
{
    private readonly IGenericRepository<CrmTask> _repository;

    public TaskService(IGenericRepository<CrmTask> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TaskResponse>> GetAllAsync()
    {
        var tasks = await _repository.GetAllAsync();
        return tasks.Select(t => new TaskResponse
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            StartDate = t.StartDate,
            DueDate = t.DueDate,
            ProjectId = t.ProjectId,
            CreatedAt = t.CreatedAt
        }).ToList();
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request)
    {
        var task = new CrmTask
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId ?? Guid.Empty,
            StartDate = request.StartDate ?? DateTime.UtcNow,
            DueDate = request.DueDate
        };

        await _repository.AddAsync(task);
        await _repository.SaveChangesAsync();

        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            StartDate = task.StartDate,
            DueDate = task.DueDate,
            ProjectId = task.ProjectId,
            CreatedAt = task.CreatedAt
        };
    }
}
