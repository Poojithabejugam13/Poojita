using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Domain.Entities;

namespace HartfordInsurance.Application.Interfaces;

public interface IAgentService
{
    Task<AgentDashboardDto> GetDashboardAsync(int agentId);

    Task<List<AgentPendingRequestDto>> GetPendingPolicyRequestsAsync(int agentId);

    Task ApprovePolicyAsync(int policyId, string? reason = null);

    Task RejectPolicyAsync(int policyId, string reason);

    Task<AgentDashboardDto> GetCommissionDetailsAsync(int agentId);

    Task<List<CustomerPolicy>> GetSoldPoliciesAsync(int agentId);

    Task<List<CustomerListItemDto>> GetMyCustomersAsync(int agentId);

    Task UpdatePolicyStatusAsync(int policyId, HartfordInsurance.Domain.Enums.PolicyStatus status);
    Task ExpirePolicyAsync(int policyId);
    Task CancelPolicyAsync(int policyId);
}