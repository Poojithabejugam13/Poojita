namespace HartfordInsurance.Application.DTOs;

public class ClaimDto
{
    public int ClaimId { get; set; }
    public int PolicyId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string TierName { get; set; } = string.Empty;
    public string PlanDescription { get; set; } = string.Empty;
    public decimal ClaimAmount { get; set; }
    public string Reason { get; set; }
    public string? DocumentUrl { get; set; }
    public string? ApprovalReason { get; set; }
    public string? RejectionReason { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPedWaitingViolated { get; set; }
}