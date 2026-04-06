using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.Invoices;
using Xunit;
using AutoFixture;
using FluentAssertions;
using MockQueryable.Moq;

namespace Crm.UnitTests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IGenericRepository<Invoice>> _repositoryMock;
    private readonly Mock<IGenericRepository<Contract>> _contractRepositoryMock;
    private readonly Mock<IGenericRepository<Project>> _projectRepositoryMock;
    private readonly Mock<IAdMetricService> _adMetricServiceMock;
    private readonly Fixture _fixture;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _repositoryMock = new Mock<IGenericRepository<Invoice>>();
        _contractRepositoryMock = new Mock<IGenericRepository<Contract>>();
        _projectRepositoryMock = new Mock<IGenericRepository<Project>>();
        _adMetricServiceMock = new Mock<IAdMetricService>();
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _service = new InvoiceService(
            _repositoryMock.Object,
            _contractRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _adMetricServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateInvoiceAndSave()
    {
        // Arrange
        var request = _fixture.Create<CreateInvoiceRequest>();

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.InvoiceNumber.Should().Be(request.InvoiceNumber);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GenerateFromContractAsync_ValidRetainer_CreatesCorrectInvoice()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var contract = _fixture.Build<Contract>()
            .With(c => c.Id, contractId)
            .With(c => c.BaseRetainer, 1000)
            .With(c => c.SuccessFeeType, SuccessFeeType.None)
            .Create();
        _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId)).ReturnsAsync(contract);

        // Act
        var result = await _service.GenerateFromContractAsync(contractId);

        // Assert
        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(1000);
        result.Items.Should().Contain(i => i.UnitPrice == 1000);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingBillingAsync_SignedContracts_GeneratesInvoices()
    {
        // Arrange
        var contracts = _fixture.Build<Contract>()
            .With(c => c.Status, ContractStatus.Signed)
            .With(c => c.LastInvoicedAt, (DateTimeOffset?)null)
            .With(c => c.StartDate, DateTime.UtcNow.AddDays(-1))
            .CreateMany(2)
            .ToList();
        
        var mock = contracts.AsQueryable().BuildMock();
        _contractRepositoryMock.Setup(r => r.AsQueryable()).Returns(mock);
        _contractRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => contracts.First(c => c.Id == id));

        // Act
        var result = await _service.ProcessPendingBillingAsync();

        // Assert
        result.Should().Be(2);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Exactly(2));
    }
}
