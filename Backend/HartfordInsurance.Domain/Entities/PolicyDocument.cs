using System.ComponentModel.DataAnnotations;
namespace HartfordInsurance.Domain.Entities;
public class Document
{
    [Key]
    public int DocumentId { get; set; }
    public int? PolicyId { get; set; }   
    public int? ClaimId { get; set; }    
    public string DocumentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}