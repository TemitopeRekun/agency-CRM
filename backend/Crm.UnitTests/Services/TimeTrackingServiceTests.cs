using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.TimeTracking;
using Xunit;
using AutoFixture;
using FluentAssertions;
using MockQueryable.Moq;

namespace Crm.UnitTests.Services;

public class TimeTrackingServiceTests
{
    private readonly Mock<IGenericRepository<TimeEntry>> _timeEntryRepositoryMock;
    private readonly Mock<IGenericRepository<ProjectMember>> _projectMemberRepositoryMock;
    private readonly Mock<IGenericRepository<Project>> _projectRepositoryMock;
    private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Fixture _fixture;
    private readonly TimeTrackingService _service;

    public TimeTrackingServiceTests()
    {
        _timeEntryRepositoryMock = new Mock<IGenericRepository<TimeEntry>>();
        _projectMemberRepositoryMock = new Mock<IGenericRepository<ProjectMember>>();
        _projectRepositoryMock = new Mock<IGenericRepository<Project>>();
        _userRepositoryMock = new Mock<IGenericRepository<User>>();
        _currentUserContextMock = new Mock<ICurrentUserContext>();
        
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _service = new TimeTrackingService(
            _timeEntryRepositoryMock.Object,
            _projectMemberRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _userRepositoryMock.Object,
            _currentUserContextMock.Object);
    }

    [Fact]
    public async Task LogTimeAsync_ShouldCreateTimeEntryAndReturnDto()
    {
        // Arrange
        var request = _fixture.Create<CreateTimeEntryRequest>();
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        _currentUserContextMock.Setup(c => c.UserId).Returns(userId);
        _currentUserContextMock.Setup(c => c.TenantId).Returns(tenantId);

        // Mock the lookup of the created entry for the return DTO
        var createdEntry = new TimeEntry { Id = Guid.NewGuid(), ProjectId = request.ProjectId };
        var mock = new List<TimeEntry> { createdEntry }.AsQueryable().BuildMock();
        _timeEntryRepositoryMock.Setup(r => r.AsQueryable()).Returns(mock);

        // Act
        var result = await _service.LogTimeAsync(request);

        // Assert
        result.Should().NotBeNull();
        _timeEntryRepositoryMock.Verify(r => r.AddAsync(It.Is<TimeEntry>(te => te.UserId == userId && te.TenantId == tenantId)), Times.Once);
        _timeEntryRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProjectTeamAsync_CalculatesCorrectTotals()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), FullName = "John Doe", HourlyRate = 100 };
        var members = new List<ProjectMember> { new ProjectMember { ProjectId = projectId, User = user, UserId = user.Id, Role = ProjectRole.Lead } };
        var timeEntries = new List<TimeEntry> { new TimeEntry { ProjectId = projectId, Hours = 5, User = user, UserId = user.Id } };

        _projectMemberRepositoryMock.Setup(r => r.AsQueryable()).Returns(members.AsQueryable().BuildMock());
        _timeEntryRepositoryMock.Setup(r => r.AsQueryable()).Returns(timeEntries.AsQueryable().BuildMock());

        // Act
        var result = await _service.GetProjectTeamAsync(projectId);

        // Assert
        result.TotalHours.Should().Be(5);
        result.EstimatedLaborCost.Should().Be(500); // 5 * 100
        result.Members.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddTeamMemberAsync_NewMember_SavesToRepository()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var request = new AddTeamMemberRequest { UserId = Guid.NewGuid(), Role = "Manager" };
        var tenantId = Guid.NewGuid();
        _currentUserContextMock.Setup(c => c.TenantId).Returns(tenantId);

        _projectMemberRepositoryMock.Setup(r => r.AsQueryable()).Returns(new List<ProjectMember>().AsQueryable().BuildMock());

        // Act
        await _service.AddTeamMemberAsync(projectId, request);

        // Assert
        _projectMemberRepositoryMock.Verify(r => r.AddAsync(It.Is<ProjectMember>(pm => pm.ProjectId == projectId && pm.TenantId == tenantId)), Times.Once);
        _projectMemberRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
