using HartfordInsurance.Application.DTOs;

namespace HartfordInsurance.Application.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync();

    Task<AdminDashboardDto> GetOverviewAsync();

    Task CreateAgentAsync(AuthDto request);
    Task DeleteAgentAsync(int agentId);
    Task CreateClaimsOfficerAsync(AuthDto request);
    Task DeleteClaimsOfficerAsync(int officerId);

    Task<int> CreatePlanWithTierAsync(CreatePlanRequestDto request);

    Task UpdatePlanAsync(int planId, PlanUpdateDto request);

    Task CreateTierAsync(PlanTierDto request);

    Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync();
    Task<List<ClaimsOfficerPerformanceDto>> GetClaimsOfficerPerformanceAsync();
    Task<List<PlanPerformanceDto>> GetPlanPerformanceAsync();
    Task<List<CustomerPerformanceDto>> GetCustomerPerformanceAsync();
}