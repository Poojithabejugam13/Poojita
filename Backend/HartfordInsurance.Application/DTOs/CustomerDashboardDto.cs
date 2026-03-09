namespace HartfordInsurance.Application.DTOs;

public class CustomerDashboardDto
{
    public int ActivePolicies { get; set; }
    public int PendingClaims { get; set; }
    public decimal TotalCoverageAmount { get; set; }
    public string AssignedAgentName { get; set; } = string.Empty;
    public string AssignedAgentPhone { get; set; } = string.Empty;
    public string AssignedOfficerName { get; set; } = string.Empty;
    public string AssignedOfficerPhone { get; set; } = string.Empty;
}