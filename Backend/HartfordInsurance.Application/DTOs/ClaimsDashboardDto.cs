namespace HartfordInsurance.Application.DTOs;

public class ClaimsDashboardDto
{
    public int PendingClaims { get; set; }
    public int ApprovedClaims { get; set; }
    public int RejectedClaims { get; set; }
}