using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.Contracts;
using Xunit;
using AutoFixture;
using FluentAssertions;

namespace Crm.UnitTests.Services;

public class ContractServiceTests
{
    private readonly Mock<IGenericRepository<Contract>> _repositoryMock;
    private readonly Mock<IGenericRepository<Project>> _projectRepositoryMock;
    private readonly Mock<IGenericRepository<Offer>> _offerRepositoryMock;
    private readonly Fixture _fixture;
    private readonly ContractService _service;

    public ContractServiceTests()
    {
        _repositoryMock = new Mock<IGenericRepository<Contract>>();
        _projectRepositoryMock = new Mock<IGenericRepository<Project>>();
        _offerRepositoryMock = new Mock<IGenericRepository<Offer>>();
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _service = new ContractService(
            _repositoryMock.Object,
            _projectRepositoryMock.Object,
            _offerRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateContractAndSave()
    {
        // Arrange
        var request = _fixture.Create<CreateContractRequest>();

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Contract>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GenerateFromProjectAsync_ValidProject_CreatesDraftContract()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = _fixture.Build<Project>().With(p => p.Id, projectId).Create();
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

        // Act
        var result = await _service.GenerateFromProjectAsync(projectId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ContractStatus.Draft);
        result.ProjectId.Should().Be(projectId);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Contract>()), Times.Once);
    }

    [Fact]
    public async Task SignContractAsync_ValidContract_UpdatesStatusToSigned()
    {
        // Arrange
        var id = Guid.NewGuid();
        var contract = _fixture.Build<Contract>().With(c => c.Id, id).With(c => c.Status, ContractStatus.Draft).Create();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(contract);

        var signature = "Digital Signature Data";
        var ip = "192.168.1.1";

        // Act
        var result = await _service.SignContractAsync(id, signature, ip);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(ContractStatus.Signed);
        result.SignatureStatus.Should().Contain(signature);
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Contract>(c => c.SignerIp == ip)), Times.Once);
    }
}
