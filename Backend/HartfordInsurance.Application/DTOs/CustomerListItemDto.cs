namespace HartfordInsurance.Application.DTOs;

public class CustomerListItemDto
{
    public int PolicyId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string TierName { get; set; } = string.Empty;
    public string PlanDescription { get; set; } = string.Empty;
    public string? DecisionReason { get; set; }
    public string LatestClaimStatus { get; set; } = "No Claims";
    public int Status { get; set; }
    public decimal CommissionAmount { get; set; }
}
