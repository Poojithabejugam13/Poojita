using HartfordInsurance.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace HartfordInsurance.Domain.Entities;
public class Payment
{
    [Key]
    public int PaymentId { get; set; }
    public int PolicyId { get; set; }   
    public decimal AmountPaid { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}