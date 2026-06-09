using System.ComponentModel.DataAnnotations;

namespace PractoBackend.Models;

public class Specialization
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(255)]
    public string? IconUrl { get; set; }

    public ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();
}
