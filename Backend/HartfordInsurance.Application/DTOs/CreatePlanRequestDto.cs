using Microsoft.AspNetCore.Http;

namespace HartfordInsurance.Application.DTOs;

public class CreatePlanRequestDto
{
    public string PlanName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PlanType { get; set; } = "Individual";
    public string TierName { get; set; } = string.Empty;
    
    public decimal BasePremium { get; set; }
    public decimal BaseCoverageAmount { get; set; }
    public bool AgeLockProtection { get; set; }
    
    public bool CoverageRestoreEnabled { get; set; }
    public int MaxRestoresPerYear { get; set; }
    public int BoosterMultiplier { get; set; }
    
    public int PreExistingDiseaseWaitingMonths { get; set; }
    public decimal CoPaymentPercentage { get; set; }
    public decimal CommissionPercentage { get; set; }
    
    public IFormFile? Image { get; set; }
}
