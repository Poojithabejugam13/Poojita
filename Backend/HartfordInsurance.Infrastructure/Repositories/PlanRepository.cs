using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace HartfordInsurance.Infrastructure.Repositories;
public class PlanRepository : IPlanRepository
{
    private readonly ApplicationDbContext _context;
    public PlanRepository(ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<int> AddPlanAsync(InsurancePlan plan)
    {
        _context.InsurancePlans.Add(plan);
        await _context.SaveChangesAsync();
        return plan.PlanId;
    }
    public async Task AddTierAsync(PlanTier tier)
    {
        _context.PlanTiers.Add(tier);
        await _context.SaveChangesAsync();
    }
    public async Task<PlanTier?> GetTierByIdAsync(int tierId)
        => await _context.PlanTiers.FindAsync(tierId);
    public async Task<List<PlanDto>> GetPlansWithTiersAsync()
    {
        var plans = await _context.InsurancePlans.ToListAsync();
        var tiers = await _context.PlanTiers.ToListAsync();

        return plans.Select(p => new PlanDto
        {
            PlanId = p.PlanId,
            PlanName = p.PlanName,
            PlanType = p.PlanType,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            Tiers = tiers.Where(t => t.PlanId == p.PlanId).Select(t => new TierDto
            {
                TierId = t.TierId,
                TierName = t.TierName,
                Description = t.Description,
                BasePremium = t.BasePremium,
                CoverageLimit = t.BaseCoverageAmount,
                AgeLockProtection = t.AgeLockProtection,
                CoverageRestoreEnabled = t.CoverageRestoreEnabled,
                MaxRestoresPerYear = t.MaxRestoresPerYear,
                BoosterMultiplier = t.BoosterMultiplier,
                PreExistingDiseaseWaitingMonths = t.PreExistingDiseaseWaitingMonths,
                CoPaymentPercentage = t.CoPaymentPercentage
            }).ToList()
        }).ToList();
    }

    public async Task<InsurancePlan?> GetByIdAsync(int planId)
        => await _context.InsurancePlans.FindAsync(planId);

    public async Task<PlanTier?> GetTierByPlanIdAsync(int planId)
        => await _context.PlanTiers.FirstOrDefaultAsync(t => t.PlanId == planId);

    public async Task UpdatePlanAsync(InsurancePlan plan)
    {
        _context.InsurancePlans.Update(plan);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTierAsync(PlanTier tier)
    {
        _context.PlanTiers.Update(tier);
        await _context.SaveChangesAsync();
    }
}