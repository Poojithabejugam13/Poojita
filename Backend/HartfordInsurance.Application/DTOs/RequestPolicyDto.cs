using System.Collections.Generic;
using System;

namespace HartfordInsurance.Application.DTOs;

// Used by CustomerController.RequestPolicy endpoint
public class RequestPolicyDto
{
    public int PlanId { get; set; }
    public int TierId { get; set; }
    public int Age { get; set; }
    public string PlanType { get; set; } = string.Empty; // Individual / Family / Senior / Specialised
    public DateTime PolicyStartDate { get; set; }

    // Proposer Details
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // Health Details — used by backend PremiumCalculator domain service
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public bool IsSmoker { get; set; }
    public string? PreExistingDiseases { get; set; }

    // Premium fields are NOT accepted from the client.
    // All premium calculation is done server-side by PremiumCalculator.cs

    public string DocumentUrl { get; set; } = string.Empty;
    public List<NomineeDto> Nominees { get; set; } = new();

    // Internal fields set by controller from auth token — NOT from client payload
    public int CustomerId { get; set; }
    public int AgentId { get; set; }
}


public class NomineeDto
{
    public string NomineeName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public decimal PercentageShare { get; set; }
}
