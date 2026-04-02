using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Crm.Application.DTOs.Contracts;

namespace Crm.Application.Services;

public class ContractPortalService : IContractPortalService
{
    private readonly IGenericRepository<Contract> _contractRepository;
    private readonly ISlackService _slackService;

    public ContractPortalService(IGenericRepository<Contract> contractRepository, ISlackService slackService)
    {
        _contractRepository = contractRepository;
        _slackService = slackService;
    }

    public async Task<ContractResponse?> GetContractByTokenAsync(Guid token)
    {
        var contract = await _contractRepository.AsQueryable()
            .FirstOrDefaultAsync(c => c.PortalToken == token);

        return contract == null ? null : MapToResponse(contract);
    }

    public async Task<ContractResponse?> SignContractAsync(Guid token, string signatureData, string signerIp)
    {
        var contract = await _contractRepository.AsQueryable()
            .FirstOrDefaultAsync(c => c.PortalToken == token);

        if (contract == null) return null;

        contract.SignatureData = signatureData;
        contract.SignedAt = DateTimeOffset.UtcNow;
        contract.SignerIp = signerIp;
        contract.Status = ContractStatus.Signed;

        await _contractRepository.UpdateAsync(contract);
        await _contractRepository.SaveChangesAsync();

        await _slackService.SendNotificationAsync($"📄 Contract Signed: {contract.Title}. Ready for project kickoff!");

        return MapToResponse(contract);
    }

    public async Task<bool> MarkViewedAsync(Guid token)
    {
        var contract = await _contractRepository.AsQueryable()
            .FirstOrDefaultAsync(c => c.PortalToken == token);

        if (contract == null) return false;

        if (!contract.HasBeenViewed)
        {
            contract.HasBeenViewed = true;
            contract.ViewedAt = DateTimeOffset.UtcNow;
            await _contractRepository.UpdateAsync(contract);
            await _contractRepository.SaveChangesAsync();
            
            await _slackService.SendNotificationAsync($"👀 Proposal Viewed: {contract.Title}. Client is currently reviewing the agreement.");
        }

        return true;
    }

    private ContractResponse MapToResponse(Contract c)
    {
        return new ContractResponse
        {
            Id = c.Id,
            Title = c.Title,
            TotalAmount = c.TotalAmount,
            Terms = c.Terms,
            Status = c.Status,
            SignedAt = c.SignedAt, 
            SignerIp = c.SignerIp,
            ClientId = c.ClientId,
            ProjectId = c.ProjectId,
            Version = c.Version,
            SignatureStatus = c.SignatureStatus,
            IsWaitingSignature = c.IsWaitingSignature,
            BaseRetainer = c.BaseRetainer,
            SuccessFeeValue = c.SuccessFeeValue,
            CreatedAt = c.CreatedAt
        };
    }
}
