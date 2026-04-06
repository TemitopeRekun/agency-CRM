using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.AdMetrics;
using Xunit;
using AutoFixture;
using FluentAssertions;
using MockQueryable.Moq;

namespace Crm.UnitTests.Services;

public class AdMetricServiceTests
{
    private readonly Mock<IGenericRepository<AdMetric>> _repositoryMock;
    private readonly Mock<IGenericRepository<Project>> _projectRepositoryMock;
    private readonly Mock<IGenericRepository<Contract>> _contractRepositoryMock;
    private readonly Mock<IGenericRepository<ProjectAdAccount>> _adAccountRepositoryMock;
    private readonly Mock<IAdPlatformClient> _platformClientMock;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Fixture _fixture;
    private readonly AdMetricService _service;

    public AdMetricServiceTests()
    {
        _repositoryMock = new Mock<IGenericRepository<AdMetric>>();
        _projectRepositoryMock = new Mock<IGenericRepository<Project>>();
        _contractRepositoryMock = new Mock<IGenericRepository<Contract>>();
        _adAccountRepositoryMock = new Mock<IGenericRepository<ProjectAdAccount>>();
        _platformClientMock = new Mock<IAdPlatformClient>();
        _currentUserContextMock = new Mock<ICurrentUserContext>();
        
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _platformClientMock.Setup(c => c.Platform).Returns(AdPlatform.Google);

        _service = new AdMetricService(
            _repositoryMock.Object,
            _projectRepositoryMock.Object,
            _contractRepositoryMock.Object,
            _adAccountRepositoryMock.Object,
            new List<IAdPlatformClient> { _platformClientMock.Object },
            _currentUserContextMock.Object);
    }

    [Fact]
    public async Task GetProjectAnalyticsAsync_ShouldCalculateCorrectMetrics()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var metrics = _fixture.Build<AdMetric>()
            .With(m => m.ProjectId, projectId)
            .With(m => m.Spend, 100)
            .CreateMany(2)
            .ToList();

        var queryableMetrics = metrics.AsQueryable().BuildMock();
        _repositoryMock.Setup(r => r.AsQueryable()).Returns(queryableMetrics);

        var contracts = _fixture.Build<Contract>()
            .With(c => c.ProjectId, projectId)
            .With(c => c.Status, ContractStatus.Signed)
            .With(c => c.TotalAmount, 500)
            .CreateMany(1)
            .ToList();
        _contractRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(contracts);

        // Act
        var result = await _service.GetProjectAnalyticsAsync(projectId);

        // Assert
        result.TotalSpend.Should().Be(200);
        result.ROAS.Should().Be(2.5m); // 500 / 200
        result.ProjectROI.Should().Be(150); // (500-200)/200 * 100
    }

    [Fact]
    public async Task SyncMetricsAsync_ShouldUpdateExistingAndAddNewMetrics()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var account = new ProjectAdAccount 
        { 
            Id = Guid.NewGuid(), 
            ProjectId = projectId, 
            IsActive = true, 
            Platform = AdPlatform.Google, 
            ExternalAccountId = "EXT123",
            TenantId = tenantId
        };
        
        _adAccountRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ProjectAdAccount> { account });

        var fetchedMetrics = new List<AdMetric> 
        { 
            new AdMetric { AdAccountId = account.Id, Spend = 50, Date = DateTime.UtcNow.Date } 
        };
        _platformClientMock.Setup(c => c.FetchDailyMetricsAsync(account.ExternalAccountId, It.IsAny<DateTime>()))
            .ReturnsAsync(fetchedMetrics);

        // Mock existing metrics to return empty list initially
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<AdMetric>());

        // Act
        await _service.SyncMetricsAsync(projectId);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<AdMetric>(m => m.Spend == 50 && m.ProjectId == projectId)), Times.AtLeastOnce);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldAssignTenantIdAndSave()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var request = _fixture.Create<CreateAdMetricRequest>();
        _currentUserContextMock.Setup(c => c.TenantId).Returns(tenantId);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AddAsync(It.Is<AdMetric>(m => m.TenantId == tenantId)), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
