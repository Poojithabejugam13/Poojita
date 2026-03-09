using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Domain.Entities;

namespace HartfordInsurance.Application.Interfaces;

public interface IPlanRepository
{
    Task<int> AddPlanAsync(InsurancePlan plan);

    Task AddTierAsync(PlanTier tier);

    Task<PlanTier?> GetTierByIdAsync(int tierId);

    Task<InsurancePlan?> GetByIdAsync(int planId);
    Task<PlanTier?> GetTierByPlanIdAsync(int planId);

    Task<List<PlanDto>> GetPlansWithTiersAsync();

    Task UpdatePlanAsync(InsurancePlan plan);
    Task UpdateTierAsync(PlanTier tier);
}