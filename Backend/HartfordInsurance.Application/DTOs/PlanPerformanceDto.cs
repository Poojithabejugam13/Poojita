namespace HartfordInsurance.Application.DTOs;

public class PlanPerformanceDto
{
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public int TotalPoliciesSold { get; set; }
    public decimal TotalRevenueGenerated { get; set; }
}
