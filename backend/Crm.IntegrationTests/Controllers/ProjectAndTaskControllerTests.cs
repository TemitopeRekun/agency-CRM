using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.Projects;
using Crm.Application.DTOs.Tasks;
using Crm.Application.DTOs.Clients;
using FluentAssertions;
using System.Net;

namespace Crm.IntegrationTests.Controllers;

public class ProjectsControllerTests : BaseIntegrationTest
{
    public ProjectsControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetProjects_Unauthorized_Returns401()
    {
        var response = await _client.GetAsync("/api/projects");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProject_Admin_CreatesAndReturnsProject()
    {
        // Arrange
        await AuthenticateAsync("Admin");
        var clientReq = new CreateClientRequest { Name = "Project Client" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", clientReq);
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        var request = new CreateProjectRequest 
        { 
            Name = "New Project", 
            ClientId = client!.Id 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be(request.Name);
    }
}

public class TasksControllerTests : BaseIntegrationTest
{
    public TasksControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateTask_ProjectManager_CreatesAndReturnsTask()
    {
        // Arrange
        await AuthenticateAsync("ProjectManager");
        
        // 1. Create client and project first
        var clientReq = new CreateClientRequest { Name = "Task Client" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", clientReq);
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        var projReq = new CreateProjectRequest { Name = "Task Project", ClientId = client!.Id };
        var projResp = await _client.PostAsJsonAsync("/api/projects", projReq);
        var proj = await projResp.Content.ReadFromJsonAsync<ProjectResponse>();

        // 2. Create task
        var request = new CreateTaskRequest 
        { 
            Title = "Task Integration Test", 
            ProjectId = proj!.Id 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<TaskResponse>();
        result.Should().NotBeNull();
        result!.Title.Should().Be(request.Title);
    }
}
