namespace HartfordInsurance.Application.DTOs;

public class CustomerPerformanceDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ActivePolicies { get; set; }
    public int TotalClaims { get; set; }
}
