using System.Collections.Generic;

namespace HartfordInsurance.Application.DTOs;

public class PlanDto
{
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PlanType { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<TierDto> Tiers { get; set; } = new();
}

public class TierDto
{
    public int TierId { get; set; }
    public string TierName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePremium { get; set; }
    public decimal CoverageLimit { get; set; }
    public bool AgeLockProtection { get; set; }
    public bool CoverageRestoreEnabled { get; set; }
    public int MaxRestoresPerYear { get; set; }
    public int BoosterMultiplier { get; set; }
    public int PreExistingDiseaseWaitingMonths { get; set; }
    public decimal CoPaymentPercentage { get; set; }
}