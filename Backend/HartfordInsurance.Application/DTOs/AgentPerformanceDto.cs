namespace HartfordInsurance.Application.DTOs;

public class AgentPerformanceDto
{
    public int AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public int PoliciesSold { get; set; }
    public decimal TotalCommissionEarned { get; set; }
}
