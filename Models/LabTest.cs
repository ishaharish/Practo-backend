using System.ComponentModel.DataAnnotations;

namespace PractoBackend.Models;

public class LabTest
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    public decimal? OriginalPrice { get; set; }
    
    [StringLength(100)]
    public string ReportTime { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string Category { get; set; } = "Individual"; // e.g. "Individual", "Package"
    
    public int TestCount { get; set; } = 1;
}
