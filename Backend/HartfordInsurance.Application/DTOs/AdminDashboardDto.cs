namespace HartfordInsurance.Application.DTOs;

public class AdminDashboardDto
{
    public int TotalAgents { get; set; }
    public int TotalClaimsOfficers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalPolicies { get; set; }
    public int PendingClaims { get; set; }
    public decimal TotalRevenue { get; set; }
}