using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.Invoices;
using Xunit;

namespace Crm.UnitTests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IGenericRepository<Invoice>> _repositoryMock;
    private readonly Mock<IGenericRepository<Contract>> _contractRepositoryMock;
    private readonly Mock<IGenericRepository<Project>> _projectRepositoryMock;
    private readonly Mock<IAdMetricService> _adMetricServiceMock;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _repositoryMock = new Mock<IGenericRepository<Invoice>>();
        _contractRepositoryMock = new Mock<IGenericRepository<Contract>>();
        _projectRepositoryMock = new Mock<IGenericRepository<Project>>();
        _adMetricServiceMock = new Mock<IAdMetricService>();
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
        var request = new CreateInvoiceRequest
        {
            InvoiceNumber = "INV-001",
            TotalAmount = 500,
            ContractId = Guid.NewGuid()
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.InvoiceNumber, result.InvoiceNumber);
        Assert.Equal(request.TotalAmount, result.TotalAmount);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
