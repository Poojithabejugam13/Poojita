namespace HartfordInsurance.Application.DTOs;

public class ClaimsOfficerPerformanceDto
{
    public int ClaimsOfficerId { get; set; }
    public string ClaimsOfficerName { get; set; } = string.Empty;
    public int ApprovedClaims { get; set; }
    public int RejectedClaims { get; set; }
    public int TotalClaimsProcessed { get; set; }
}
