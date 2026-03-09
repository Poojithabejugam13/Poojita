using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using HartfordInsurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HartfordInsurance.Infrastructure.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly ApplicationDbContext _context;

    public PolicyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CustomerPolicy policy)
    {
        _context.CustomerPolicies.Add(policy);
        await _context.SaveChangesAsync();
    }

    public async Task AddNomineeAsync(Nominee nominee)
    {
        _context.Nominees.Add(nominee);
        await _context.SaveChangesAsync();
    }

    public async Task<CustomerPolicy?> GetByIdAsync(int policyId)
        => await _context.CustomerPolicies.FindAsync(policyId);

    public async Task UpdateAsync(CustomerPolicy policy)
    {
        _context.CustomerPolicies.Update(policy);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountActivePoliciesAsync(int customerId)
        => await _context.CustomerPolicies
            .CountAsync(p => p.CustomerId == customerId && p.Status == PolicyStatus.Active);

    public async Task<decimal> GetTotalCoverageAsync(int customerId)
        => await _context.CustomerPolicies
            .Where(p => p.CustomerId == customerId && p.Status == PolicyStatus.Active)
            .SumAsync(p => p.RemainingCoverageAmount);

    public async Task<int> CountPoliciesByAgentAsync(int agentId)
        => await _context.CustomerPolicies.CountAsync(p => p.AgentId == agentId);

    public async Task<decimal> CalculateCommissionAsync(int agentId)
        => await _context.CustomerPolicies
            .Where(p => p.AgentId == agentId && p.Status == PolicyStatus.Active)
            .SumAsync(p => p.CommissionAmount);

    public async Task ActivatePolicyAsync(int policyId)
    {
        var policy = await GetByIdAsync(policyId);
        if (policy != null)
        {
            policy.Status = PolicyStatus.Active;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> CountAllAsync()
        => await _context.CustomerPolicies.CountAsync();

    public async Task<decimal> GetTotalRevenueAsync()
        => await _context.Payments
            .Where(p => p.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(p => p.AmountPaid);

    public async Task<List<AgentPendingRequestDto>> GetPendingPoliciesAsync(int agentId)
    {
        var policies = await (from p in _context.CustomerPolicies
                              join c in _context.Users on p.CustomerId equals c.Id
                              join plan in _context.InsurancePlans on p.PlanId equals plan.PlanId
                              where p.Status == PolicyStatus.PendingApproval && p.AgentId == agentId
                              select new AgentPendingRequestDto
                              {
                                  PolicyId = p.PolicyId,
                                  PolicyNumber = p.PolicyNumber,
                                  CustomerName = c.FullName,
                                  PlanName = plan.PlanName,
                                  PlanDescription = plan.Description,
                                  TotalPremium = p.TotalPremium,
                                  CommissionAmount = p.CommissionAmount,
                                  Status = (int)p.Status,
                                  CreatedAt = p.CreatedAt,
                                  PlanType = p.PlanType,
                                  PolicyStartDate = p.PolicyStartDate,
                                  FullName = p.FullName,
                                  DateOfBirth = p.DateOfBirth,
                                  Gender = p.Gender,
                                  MobileNumber = p.MobileNumber,
                                  Address = p.Address,
                                  HeightCm = p.HeightCm,
                                  WeightKg = p.WeightKg,
                                  IsSmoker = p.IsSmoker,
                                  DocumentUrl = _context.Documents
                                                .Where(d => d.PolicyId == p.PolicyId)
                                                .Select(d => d.FilePath)
                                                .FirstOrDefault()
                              }).ToListAsync();

        var policyIds = policies.Select(p => p.PolicyId).ToList();
        var allNominees = await _context.Nominees
            .Where(n => policyIds.Contains(n.PolicyId))
            .ToListAsync();

        foreach (var policy in policies)
        {
            policy.Nominees = allNominees
                .Where(n => n.PolicyId == policy.PolicyId)
                .Select(n => new NomineeDto
                {
                    NomineeName = n.NomineeName,
                    Relationship = n.Relationship,
                    PercentageShare = n.PercentageShare
                }).ToList();
        }

        return policies;
    }

    public async Task<List<CustomerPolicy>> GetPoliciesByAgentAsync(int agentId)
        => await _context.CustomerPolicies
            .Where(p => p.AgentId == agentId)
            .ToListAsync();

    public async Task<List<CustomerPolicy>> GetPoliciesByCustomerAsync(int customerId)
        => await _context.CustomerPolicies
            .Where(p => p.CustomerId == customerId)
            .ToListAsync();

    public async Task<List<CustomerPolicy>> GetPoliciesByClaimsOfficerAsync(int officerId)
        => await _context.CustomerPolicies
            .Where(p => p.ClaimsOfficerId == officerId)
            .ToListAsync();

    public async Task AddPaymentAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Nominee>> GetNomineesByPolicyIdAsync(int policyId)
        => await _context.Nominees.Where(n => n.PolicyId == policyId).ToListAsync();

    public async Task<List<PlanPerformanceDto>> GetPlanPerformanceAsync()
    {
        return await (from plan in _context.InsurancePlans
                      join policy in _context.CustomerPolicies on plan.PlanId equals policy.PlanId into policies
                      select new PlanPerformanceDto
                      {
                          PlanId = plan.PlanId,
                          PlanName = plan.PlanName,
                          PlanType = plan.PlanType,
                          TotalPoliciesSold = policies.Count(),
                          TotalRevenueGenerated = policies.Sum(p => p.TotalPremium)
                      }).ToListAsync();
    }
}