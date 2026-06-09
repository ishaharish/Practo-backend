using System.ComponentModel.DataAnnotations;

namespace PractoBackend.Models;

public class SurgeryType
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;
    
    public decimal EstimatedCost { get; set; }
}
