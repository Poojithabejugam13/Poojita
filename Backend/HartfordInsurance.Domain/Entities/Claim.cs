using HartfordInsurance.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace HartfordInsurance.Domain.Entities;
public class Claim
{
    [Key]
    public int ClaimId { get; set; }
    public int PolicyId { get; set; }
    public decimal ClaimAmount { get; set; }
    public string ClaimReason { get; set; } = string.Empty;
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;
    public int? ProcessedBy { get; set; }    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? ApprovalReason { get; set; }
}