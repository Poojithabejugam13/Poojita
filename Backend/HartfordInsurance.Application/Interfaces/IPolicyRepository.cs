using System.Collections.Generic;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Application.DTOs;

namespace HartfordInsurance.Application.Interfaces;

public interface IPolicyRepository
{
    Task AddAsync(CustomerPolicy policy);
    Task AddNomineeAsync(Nominee nominee);

    Task<CustomerPolicy?> GetByIdAsync(int policyId);

    Task UpdateAsync(CustomerPolicy policy);

    Task<int> CountActivePoliciesAsync(int customerId);

    Task<decimal> GetTotalCoverageAsync(int customerId);

    Task<int> CountPoliciesByAgentAsync(int agentId);

    Task<decimal> CalculateCommissionAsync(int agentId);

    Task ActivatePolicyAsync(int policyId);

    Task<int> CountAllAsync();

    Task<decimal> GetTotalRevenueAsync();

    Task<List<AgentPendingRequestDto>> GetPendingPoliciesAsync(int agentId);

    Task<List<CustomerPolicy>> GetPoliciesByAgentAsync(int agentId);

    Task<List<CustomerPolicy>> GetPoliciesByCustomerAsync(int customerId);

    Task<List<CustomerPolicy>> GetPoliciesByClaimsOfficerAsync(int officerId);

    Task AddPaymentAsync(Payment payment);
    Task<List<Nominee>> GetNomineesByPolicyIdAsync(int policyId);
    Task<List<PlanPerformanceDto>> GetPlanPerformanceAsync();
}