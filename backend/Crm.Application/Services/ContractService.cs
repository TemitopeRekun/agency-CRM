using Crm.Application.DTOs.Contracts;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;

namespace Crm.Application.Services;

public class ContractService
{
    private readonly IGenericRepository<Contract> _repository;
    private readonly IGenericRepository<Project> _projectRepository;
    private readonly IGenericRepository<Offer> _offerRepository;

    public ContractService(
        IGenericRepository<Contract> repository,
        IGenericRepository<Project> projectRepository,
        IGenericRepository<Offer> offerRepository)
    {
        _repository = repository;
        _projectRepository = projectRepository;
        _offerRepository = offerRepository;
    }

    public async Task<IEnumerable<ContractResponse>> GetAllAsync()
    {
        var contracts = await _repository.GetAllAsync();
        return contracts.Select(MapToResponse).ToList();
    }

    public async Task<ContractResponse> CreateAsync(CreateContractRequest request)
    {
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            TotalAmount = request.TotalAmount,
            Terms = request.Terms,
            Kpis = request.Kpis,
            ProjectId = request.ProjectId,
            ClientId = request.ClientId,
            Status = ContractStatus.Draft,
            BaseRetainer = request.BaseRetainer,
            SuccessFeeType = request.SuccessFeeType,
            SuccessFeeValue = request.SuccessFeeValue
        };

        await _repository.AddAsync(contract);
        await _repository.SaveChangesAsync();

        return MapToResponse(contract);
    }

    public async Task<ContractResponse> GenerateFromProjectAsync(Guid projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId) 
            ?? throw new KeyNotFoundException("Project not found");
        
        var offer = project.OfferId.HasValue 
            ? await _offerRepository.GetByIdAsync(project.OfferId.Value)
            : null;

        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            Title = $"Contract for {project.Name}",
            ProjectId = projectId,
            ClientId = project.ClientId ?? Guid.Empty,
            TotalAmount = offer?.TotalAmount ?? 0,
            Terms = "Standard Agency Terms Apply. Service includes: " + project.Description,
            Kpis = "1. Delivery within timeline\n2. Quality assurance passed",
            Status = ContractStatus.Draft,
            Version = 1,
            SignatureStatus = "Awaiting Generation"
        };

        await _repository.AddAsync(contract);
        await _repository.SaveChangesAsync();

        return MapToResponse(contract);
    }

    public async Task<ContractResponse?> SignContractAsync(Guid id, string digitalSignature, string signerIp)
    {
        var contract = await _repository.GetByIdAsync(id);
        if (contract == null) return null;

        contract.Status = ContractStatus.Signed;
        contract.SignatureStatus = $"Signed by {digitalSignature}";
        contract.IsWaitingSignature = false;
        contract.SignedAt = DateTimeOffset.UtcNow;
        contract.SignatureData = digitalSignature;
        contract.SignerIp = signerIp;

        await _repository.UpdateAsync(contract);
        await _repository.SaveChangesAsync();

        return MapToResponse(contract);
    }

    private ContractResponse MapToResponse(Contract c)
    {
        return new ContractResponse
        {
            Id = c.Id,
            Title = c.Title,
            TotalAmount = c.TotalAmount,
            Status = c.Status,
            ProjectId = c.ProjectId,
            ClientId = c.ClientId,
            Version = c.Version,
            SignatureStatus = c.SignatureStatus,
            IsWaitingSignature = c.IsWaitingSignature,
            SignedAt = c.SignedAt,
            SignerIp = c.SignerIp,
            BaseRetainer = c.BaseRetainer,
            SuccessFeeType = c.SuccessFeeType,
            SuccessFeeValue = c.SuccessFeeValue,
            LastInvoicedAt = c.LastInvoicedAt,
            CreatedAt = c.CreatedAt
        };
    }
}
