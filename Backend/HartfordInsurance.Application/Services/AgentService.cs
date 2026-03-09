using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;

namespace HartfordInsurance.Application.Services;

public class AgentService : IAgentService
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPlanRepository _planRepository;

    public AgentService(IPolicyRepository policyRepository, IUserRepository userRepository, IPlanRepository planRepository)
    {
        _policyRepository = policyRepository;
        _userRepository = userRepository;
        _planRepository = planRepository;
    }

    public async Task<AgentDashboardDto> GetDashboardAsync(int agentId)
        => new AgentDashboardDto
        {
            PoliciesSold          = await _policyRepository.CountPoliciesByAgentAsync(agentId),
            TotalCommissionEarned = await _policyRepository.CalculateCommissionAsync(agentId)
        };

    public async Task<List<AgentPendingRequestDto>> GetPendingPolicyRequestsAsync(int agentId)
        => await _policyRepository.GetPendingPoliciesAsync(agentId);

    public async Task ApprovePolicyAsync(int policyId, string? reason = null)
    {
        var policy = await _policyRepository.GetByIdAsync(policyId)
            ?? throw new Exception($"Policy {policyId} not found.");

        if (policy.Status != PolicyStatus.PendingApproval)
            throw new Exception("Policy is not in PendingApproval state.");

        policy.Status = PolicyStatus.Approved;
        policy.DecisionReason = reason;
        await _policyRepository.UpdateAsync(policy);
    }

    public async Task RejectPolicyAsync(int policyId, string reason)
    {
        var policy = await _policyRepository.GetByIdAsync(policyId)
            ?? throw new Exception($"Policy {policyId} not found.");

        policy.Status = PolicyStatus.Rejected;
        policy.RejectionReason = reason;
        policy.DecisionReason = reason;
        await _policyRepository.UpdateAsync(policy);
    }

    public async Task<AgentDashboardDto> GetCommissionDetailsAsync(int agentId)
        => new AgentDashboardDto
        {
            PoliciesSold          = await _policyRepository.CountPoliciesByAgentAsync(agentId),
            TotalCommissionEarned = await _policyRepository.CalculateCommissionAsync(agentId)
        };

    public async Task<List<CustomerPolicy>> GetSoldPoliciesAsync(int agentId)
        => await _policyRepository.GetPoliciesByAgentAsync(agentId);

    public async Task<List<CustomerListItemDto>> GetMyCustomersAsync(int agentId)
    {
        var policies = await _policyRepository.GetPoliciesByAgentAsync(agentId);
        var result = new List<CustomerListItemDto>();

        foreach (var p in policies)
        {
            var user = await _userRepository.GetByIdAsync(p.CustomerId);
            var plan = (await _planRepository.GetPlansWithTiersAsync()).FirstOrDefault(pl => pl.PlanId == p.PlanId);

            if (user != null)
            {
                result.Add(new CustomerListItemDto
                {
                    PolicyId = p.PolicyId,
                    CustomerId = user.Id,
                    CustomerName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber ?? "N/A",
                    PolicyNumber = p.PolicyNumber,
                    PlanName = plan?.PlanName ?? "Unknown Plan",
                    PlanDescription = plan?.Description ?? string.Empty,
                    DecisionReason = p.DecisionReason,
                    Status = (int)p.Status,
                    CommissionAmount = p.CommissionAmount
                });
            }
        }

        return result;
    }

    public async Task UpdatePolicyStatusAsync(int policyId, PolicyStatus status)
    {
        var policy = await _policyRepository.GetByIdAsync(policyId)
            ?? throw new Exception($"Policy {policyId} not found.");

        policy.Status = status;
        await _policyRepository.UpdateAsync(policy);
    }

    public async Task ExpirePolicyAsync(int policyId)
    {
        var policy = await _policyRepository.GetByIdAsync(policyId)
            ?? throw new Exception($"Policy {policyId} not found.");

        policy.Status = PolicyStatus.Expired;
        await _policyRepository.UpdateAsync(policy);
    }

    public async Task CancelPolicyAsync(int policyId)
    {
        var policy = await _policyRepository.GetByIdAsync(policyId)
            ?? throw new Exception($"Policy {policyId} not found.");

        policy.Status = PolicyStatus.Cancelled;
        await _policyRepository.UpdateAsync(policy);
    }
}