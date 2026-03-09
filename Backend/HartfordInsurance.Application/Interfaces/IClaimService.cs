using HartfordInsurance.Application.DTOs;

namespace HartfordInsurance.Application.Interfaces;

public interface IClaimService
{
    Task<ClaimsDashboardDto> GetDashboardAsync();

    Task<List<ClaimDto>> GetPendingClaimsAsync();

    Task ApproveClaimAsync(int claimId, string? approvalReason = null);

    Task RejectClaimAsync(int claimId, string reason);

    Task<List<CustomerListItemDto>> GetMyCustomersAsync(int claimsOfficerId);
}