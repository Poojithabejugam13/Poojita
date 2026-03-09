using HartfordInsurance.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace HartfordInsurance.Domain.Entities;
public class CustomerPolicy
{
    [Key]
    public int PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public int PlanId { get; set; }
    public int TierId { get; set; }  
    public int CustomerId { get; set; }  
    public int AgentId { get; set; }     
    public int ClaimsOfficerId { get; set; }
    public int EntryAge { get; set; }
    public decimal BasePremium { get; set; }
    public decimal AgeLoading { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalPremium { get; set; }
    public decimal CommissionPercentage { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal AnnualMaxCoverage { get; set; }
    public decimal RemainingCoverageAmount { get; set; }
    public int RestoresUsedThisYear { get; set; }
    public string NomineeName { get; set; } = string.Empty;
    public string NomineeRelation { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public PolicyStatus Status { get; set; } = PolicyStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Simplified Insurance Flow Fields
    public string PlanType { get; set; } = string.Empty;
    public DateTime PolicyStartDate { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public bool IsSmoker { get; set; }
    public string? PreExistingDiseases { get; set; }
    public string? RejectionReason { get; set; }
    public string? DecisionReason { get; set; }
}