using Crm.Application.DTOs.Projects;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;

namespace Crm.Application.Services;

public class ProjectService
{
    private readonly IGenericRepository<Project> _repository;

    public ProjectService(IGenericRepository<Project> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProjectResponse>> GetAllAsync()
    {
        var projects = await _repository.GetAllAsync();
        return projects.Select(p => new ProjectResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            ClientId = p.ClientId,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ClientId = request.ClientId,
            OfferId = request.OfferId
        };

        await _repository.AddAsync(project);
        await _repository.SaveChangesAsync();

        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            ClientId = project.ClientId,
            CreatedAt = project.CreatedAt
        };
    }
}
