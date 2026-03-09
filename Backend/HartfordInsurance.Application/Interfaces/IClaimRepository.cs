using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;

namespace HartfordInsurance.Application.Interfaces;

public interface IClaimRepository
{
    Task AddAsync(Claim claim);

    Task<Claim?> GetByIdAsync(int claimId);

    Task UpdateAsync(Claim claim);

    Task<int> CountPendingClaimsAsync(int customerId);

    Task<int> CountByStatusAsync(ClaimStatus status);

    Task<List<ClaimDto>> GetPendingClaimDtosAsync();

    Task<List<ClaimDto>> GetClaimsByCustomerAsync(int customerId);

    Task<List<Claim>> GetClaimsByPolicyAsync(int policyId);

    Task AddDocumentAsync(Document doc);
}