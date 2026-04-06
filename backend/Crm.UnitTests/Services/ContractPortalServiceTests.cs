using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.Contracts;
using Xunit;
using AutoFixture;
using FluentAssertions;
using MockQueryable.Moq;

namespace Crm.UnitTests.Services;

public class ContractPortalServiceTests
{
    private readonly Mock<IGenericRepository<Contract>> _contractRepositoryMock;
    private readonly Mock<ISlackService> _slackServiceMock;
    private readonly Fixture _fixture;
    private readonly ContractPortalService _service;

    public ContractPortalServiceTests()
    {
        _contractRepositoryMock = new Mock<IGenericRepository<Contract>>();
        _slackServiceMock = new Mock<ISlackService>();
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _service = new ContractPortalService(
            _contractRepositoryMock.Object,
            _slackServiceMock.Object);
    }

    [Fact]
    public async Task GetContractByTokenAsync_TokenExists_ReturnsContract()
    {
        // Arrange
        var token = Guid.NewGuid();
        var contract = _fixture.Build<Contract>().With(c => c.PortalToken, token).Create();
        var mock = new List<Contract> { contract }.AsQueryable().BuildMock();
        
        _contractRepositoryMock.Setup(r => r.AsQueryable()).Returns(mock);

        // Act
        var result = await _service.GetContractByTokenAsync(token);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(contract.Id);
    }

    [Fact]
    public async Task SignContractAsync_TokenExists_UpdatesStatusAndNotifies()
    {
        // Arrange
        var token = Guid.NewGuid();
        var contract = _fixture.Build<Contract>().With(c => c.PortalToken, token).With(c => c.Status, ContractStatus.Draft).Create();
        var mock = new List<Contract> { contract }.AsQueryable().BuildMock();
        
        _contractRepositoryMock.Setup(r => r.AsQueryable()).Returns(mock);

        // Act
        var result = await _service.SignContractAsync(token, "Signer Signature", "127.0.0.1");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(ContractStatus.Signed);
        _slackServiceMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>()), Times.Once);
        _contractRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Once);
        _contractRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task MarkViewedAsync_FirstTimeView_UpdatesAndNotifies()
    {
        // Arrange
        var token = Guid.NewGuid();
        var contract = _fixture.Build<Contract>().With(c => c.PortalToken, token).With(c => c.HasBeenViewed, false).Create();
        var mock = new List<Contract> { contract }.AsQueryable().BuildMock();
        
        _contractRepositoryMock.Setup(r => r.AsQueryable()).Returns(mock);

        // Act
        var result = await _service.MarkViewedAsync(token);

        // Assert
        result.Should().BeTrue();
        _slackServiceMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>()), Times.Once);
        _contractRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Contract>(c => c.HasBeenViewed == true)), Times.Once);
    }
}
