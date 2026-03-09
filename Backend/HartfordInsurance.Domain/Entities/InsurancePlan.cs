using System.ComponentModel.DataAnnotations;
namespace HartfordInsurance.Domain.Entities;
public class InsurancePlan
{
    [Key]
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int CreatedBy { get; set; }   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}