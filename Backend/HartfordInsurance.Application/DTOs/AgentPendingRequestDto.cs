using System;
using System.Collections.Generic;

namespace HartfordInsurance.Application.DTOs;

public class AgentPendingRequestDto
{
    public int PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string PlanDescription { get; set; } = string.Empty;
    public string? DecisionReason { get; set; }
    public decimal TotalPremium { get; set; }
    public decimal CommissionAmount { get; set; }
    
    // Verification Fields
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

    public List<NomineeDto> Nominees { get; set; } = new();
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? DocumentUrl { get; set; }
}
