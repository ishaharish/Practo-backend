using System.ComponentModel.DataAnnotations;

namespace PractoBackend.Models;

public class SurgeryEnquiry
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(15)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
    
    public int SurgeryTypeId { get; set; }
    
    public SurgeryType? SurgeryType { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
