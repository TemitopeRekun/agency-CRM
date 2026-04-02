using Crm.Application.DTOs.Contracts;

namespace Crm.Application.Interfaces;

public interface IContractPortalService
{
    Task<ContractResponse?> GetContractByTokenAsync(Guid token);
    Task<ContractResponse?> SignContractAsync(Guid token, string signatureData, string signerIp);
    Task<bool> MarkViewedAsync(Guid token);
}
