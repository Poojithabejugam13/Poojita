using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using HartfordInsurance.Domain.Services;
using HartfordInsurance.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HartfordInsurance.Application.Services;

public class ClaimService : IClaimService
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPlanRepository _planRepository;

    public ClaimService(IClaimRepository claimRepository, IPolicyRepository policyRepository, IUserRepository userRepository, IPlanRepository planRepository)
    {
        _claimRepository = claimRepository;
        _policyRepository = policyRepository;
        _userRepository = userRepository;
        _planRepository = planRepository;
    }

    public async Task<ClaimsDashboardDto> GetDashboardAsync()
        => new ClaimsDashboardDto
        {
            PendingClaims  = await _claimRepository.CountByStatusAsync(ClaimStatus.Pending),
            ApprovedClaims = await _claimRepository.CountByStatusAsync(ClaimStatus.Approved),
            RejectedClaims = await _claimRepository.CountByStatusAsync(ClaimStatus.Rejected)
        };

    public async Task<List<ClaimDto>> GetPendingClaimsAsync()
    {
        var dtos = await _claimRepository.GetPendingClaimDtosAsync();
        foreach (var dto in dtos)
        {
            var policy = await _policyRepository.GetByIdAsync(dto.PolicyId);
            if (policy == null) continue;

            // Plan is no longer needed here as ExemptKeywords logic is removed
            // var plan = await _planRepository.GetByIdAsync(policy.PlanId);
            // if (plan == null) continue;

            var tier = await _planRepository.GetTierByIdAsync(policy.TierId);
            if (tier == null) continue;

            DateTime eligibleDate = policy.IssueDate.AddMonths(tier.PreExistingDiseaseWaitingMonths);
            if (DateTime.UtcNow < eligibleDate)
            {
                dto.IsPedWaitingViolated = true;
            }
        }
        return dtos;
    }

    public async Task ApproveClaimAsync(int claimId, string? approvalReason = null)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId)
            ?? throw new Exception($"Claim {claimId} not found.");

        var policy = await _policyRepository.GetByIdAsync(claim.PolicyId)
            ?? throw new Exception("Policy not found for claim.");

        // Plan is no longer needed here as ExemptKeywords logic is removed
        // var plan = await _planRepository.GetByIdAsync(policy.PlanId)
        //     ?? throw new Exception("Plan not found for policy.");

        var tier = await _planRepository.GetTierByIdAsync(policy.TierId)
            ?? throw new Exception("Tier not found for policy.");

        // 1. DOMAIN SERVICE: Enforce PED Waiting Period (Pure Officer Judgment)
        DateTime eligibleDate = policy.IssueDate.AddMonths(tier.PreExistingDiseaseWaitingMonths);
        if (DateTime.UtcNow < eligibleDate)
        {
            // If within waiting period, strictly require officer justification
            if (string.IsNullOrEmpty(approvalReason))
            {
                throw new Exception("Policy is within the Waiting Period. Manual verification of medical docs and officer justification is required for approval.");
            }
        }

        // 2. DOMAIN SERVICE: Coverage Restore (if needed)
    // Rule 7 & 9: If coverage = 0, restore. If no restore available and coverage = 0, reject claim.
    if (policy.RemainingCoverageAmount == 0)
    {
        if (CoverageManager.CanRestore(tier.CoverageRestoreEnabled, policy.RestoresUsedThisYear, tier.MaxRestoresPerYear))
        {
            policy.RemainingCoverageAmount = policy.AnnualMaxCoverage;
            policy.RestoresUsedThisYear++;
        }
        else
        {
            throw new Exception("Claim rejected: Coverage exhausted and restore not available.");
        }
    }

        // 3. DOMAIN SERVICE: Process claim via ClaimValidator (handles co-payment, capping, and coverage calculation)
        var result = ClaimValidator.ProcessClaim(claim.ClaimAmount, policy.RemainingCoverageAmount, tier.CoPaymentPercentage);
        
        policy.RemainingCoverageAmount = result.RemainingCoverage;
        claim.Status = ClaimStatus.Approved;
        claim.ApprovalReason = approvalReason;
        claim.ProcessedAt = DateTime.UtcNow;
        
        // Let's store the actual insurance paid amount if needed, though the entity doesn't have a specific field for it beside status.
        // If we want to record the payout, we might need a new field, but sticking to existing schema for now.
        
        await _policyRepository.UpdateAsync(policy);
        await _claimRepository.UpdateAsync(claim);
    }

    public async Task RejectClaimAsync(int claimId, string reason)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId)
            ?? throw new Exception($"Claim {claimId} not found.");

        claim.Status = ClaimStatus.Rejected;
        claim.RejectionReason = reason;
        await _claimRepository.UpdateAsync(claim);
    }

    public async Task<List<CustomerListItemDto>> GetMyCustomersAsync(int claimsOfficerId)
    {
        var policies = await _policyRepository.GetPoliciesByClaimsOfficerAsync(claimsOfficerId);
        var result = new List<CustomerListItemDto>();

        foreach (var p in policies)
        {
            var user = await _userRepository.GetByIdAsync(p.CustomerId);
            var plan = (await _planRepository.GetPlansWithTiersAsync()).FirstOrDefault(pl => pl.PlanId == p.PlanId);

            if (user != null)
            {
                var tierName = plan?.Tiers?.FirstOrDefault(t => t.TierId == p.TierId)?.TierName ?? "Unknown Tier";
                
                // Fetch the latest claim for this policy
                var claims = await _claimRepository.GetClaimsByPolicyAsync(p.PolicyId);
                var latestClaim = claims.OrderByDescending(c => c.CreatedAt).FirstOrDefault();
                string claimStatusStr = "No Claims";
                
                if (latestClaim != null)
                {
                    claimStatusStr = latestClaim.Status switch
                    {
                        ClaimStatus.Pending => "Pending",
                        ClaimStatus.Approved => "Approved",
                        ClaimStatus.Rejected => "Rejected",
                        _ => "Unknown"
                    };
                }

                result.Add(new CustomerListItemDto
                {
                    PolicyId = p.PolicyId,
                    CustomerId = user.Id,
                    CustomerName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = p.MobileNumber ?? "N/A", // Using MobileNumber from Policy as requested
                    PolicyNumber = p.PolicyNumber,
                    PlanName = plan?.PlanName ?? "Unknown Plan",
                    TierName = tierName,
                    PlanDescription = plan?.Description ?? string.Empty,
                    DecisionReason = p.DecisionReason,
                    LatestClaimStatus = claimStatusStr,
                    Status = (int)p.Status
                });
            }
        }

        return result;
    }
}