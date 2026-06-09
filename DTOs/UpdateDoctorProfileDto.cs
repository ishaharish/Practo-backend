using System.ComponentModel.DataAnnotations;

namespace PractoBackend.DTOs;

public class UpdateDoctorProfileDto
{
    [Required]
    public int ExperienceYears { get; set; }

    [Required]
    [MaxLength(255)]
    public string Qualifications { get; set; } = string.Empty;

    public string? Biography { get; set; }

    public decimal ConsultationFee { get; set; }

    public List<int> ClinicIds { get; set; } = new();

    public List<int> SpecializationIds { get; set; } = new();
}
