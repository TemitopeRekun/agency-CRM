using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.TimeEntries;
using Crm.Application.DTOs.TimeTracking;
using Crm.Application.DTOs.Projects;
using Crm.Application.DTOs.Tasks;
using Crm.Application.DTOs.Clients;
using FluentAssertions;
using System.Net;

namespace Crm.IntegrationTests.Controllers;

public class TimeTrackingControllerTests : BaseIntegrationTest
{
    public TimeTrackingControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task LogTime_ValidRequest_CreatesEntry()
    {
        // Arrange
        await AuthenticateAsync("Admin");
        var clientReq = new CreateClientRequest { Name = "TT Client" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", clientReq);
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        var projReq = new CreateProjectRequest { Name = "TT Proj", ClientId = client!.Id };
        var projResp = await _client.PostAsJsonAsync("/api/projects", projReq);
        var proj = await projResp.Content.ReadFromJsonAsync<ProjectResponse>();

        var taskReq = new CreateTaskRequest { Title = "TT Task", ProjectId = proj!.Id };
        var taskResp = await _client.PostAsJsonAsync("/api/tasks", taskReq);
        var task = await taskResp.Content.ReadFromJsonAsync<TaskResponse>();

        var request = new Crm.Application.DTOs.TimeTracking.CreateTimeEntryRequest 
        { 
            ProjectId = proj.Id, 
            TaskId = task!.Id,
            Hours = 4,
            Description = "Coding session"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/timetracking", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Crm.Application.DTOs.TimeTracking.TimeEntryDto>();
        result.Should().NotBeNull();
        result!.Hours.Should().Be(4);
    }
}

public class TimeEntriesControllerTests : BaseIntegrationTest
{
    public TimeEntriesControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAllEntries_Authorized_ReturnsList()
    {
        // Arrange
        await AuthenticateAsync("Admin");

        // Act
        var response = await _client.GetAsync("/api/timeentries");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<TimeEntryResponse>>();
        result.Should().NotBeNull();
    }
}
