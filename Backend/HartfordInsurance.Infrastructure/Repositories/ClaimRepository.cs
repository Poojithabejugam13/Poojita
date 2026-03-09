using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using HartfordInsurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace HartfordInsurance.Infrastructure.Repositories;
public class ClaimRepository : IClaimRepository
{
    private readonly ApplicationDbContext _context;
    public ClaimRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(Claim claim)
    {
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();
    }
    public async Task<Claim?> GetByIdAsync(int claimId)
        => await _context.Claims.FindAsync(claimId);
    public async Task UpdateAsync(Claim claim)
    {
        _context.Claims.Update(claim);
        await _context.SaveChangesAsync();
    }
    public async Task<int> CountPendingClaimsAsync(int customerId)
        => await (
            from c in _context.Claims
            join p in _context.CustomerPolicies on c.PolicyId equals p.PolicyId
            where p.CustomerId == customerId && c.Status == ClaimStatus.Pending
            select c
        ).CountAsync();
    public async Task<int> CountByStatusAsync(ClaimStatus status)
        => await _context.Claims.CountAsync(c => c.Status == status);
    public async Task<List<ClaimDto>> GetPendingClaimDtosAsync()
        => await (from c in _context.Claims
                  join p in _context.CustomerPolicies on c.PolicyId equals p.PolicyId
                  join plan in _context.InsurancePlans on p.PlanId equals plan.PlanId
                  join tier in _context.PlanTiers on p.TierId equals tier.TierId
                  where c.Status == ClaimStatus.Pending
                  select new ClaimDto
                  {
                      ClaimId = c.ClaimId,
                      PolicyId = c.PolicyId,
                      PlanName = plan.PlanName,
                      TierName = tier.TierName,
                      PlanDescription = plan.Description,
                      ClaimAmount = c.ClaimAmount,
                      Reason = c.ClaimReason,
                      DocumentUrl = _context.Documents.Where(d => d.ClaimId == c.ClaimId).Select(d => d.FilePath).FirstOrDefault(),
                      ApprovalReason = c.ApprovalReason,
                      RejectionReason = c.RejectionReason,
                      Status = (int)c.Status,
                      CreatedAt = c.CreatedAt
                  }).ToListAsync();
    public async Task<List<ClaimDto>> GetClaimsByCustomerAsync(int customerId)
        => await (from c in _context.Claims
                  join p in _context.CustomerPolicies on c.PolicyId equals p.PolicyId
                  join plan in _context.InsurancePlans on p.PlanId equals plan.PlanId
                  join tier in _context.PlanTiers on p.TierId equals tier.TierId
                  where p.CustomerId == customerId
                  select new ClaimDto
                  {
                      ClaimId = c.ClaimId,
                      PolicyId = c.PolicyId,
                      PlanName = plan.PlanName,
                      TierName = tier.TierName,
                      PlanDescription = plan.Description, // Retaining for DTO consistency, but user requested not to SHOW in claim view
                      ClaimAmount = c.ClaimAmount,
                      Reason = c.ClaimReason,
                      DocumentUrl = _context.Documents.Where(d => d.ClaimId == c.ClaimId).Select(d => d.FilePath).FirstOrDefault(),
                      ApprovalReason = c.ApprovalReason,
                      RejectionReason = c.RejectionReason,
                      Status = (int)c.Status,
                      CreatedAt = c.CreatedAt
                  }).ToListAsync();

    public async Task<List<Claim>> GetClaimsByPolicyAsync(int policyId)
        => await _context.Claims
            .Where(c => c.PolicyId == policyId)
            .ToListAsync();

    public async Task AddDocumentAsync(Document doc)
    {
        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();
    }
}