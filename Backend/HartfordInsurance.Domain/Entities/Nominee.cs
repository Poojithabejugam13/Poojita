using System.ComponentModel.DataAnnotations;
namespace HartfordInsurance.Domain.Entities;
public class Nominee
{
    [Key]
    public int NomineeId { get; set; }
    public int PolicyId { get; set; }    
    public string NomineeName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public int NomineeAge { get; set; }
    public decimal PercentageShare { get; set; }
}