using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PractoBackend.Models;

public class DoctorProfile
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public int ExperienceYears { get; set; }

    [Required]
    [MaxLength(255)]
    public string Qualifications { get; set; } = string.Empty;

    public string? Biography { get; set; }

    [MaxLength(100)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(255)]
    public string? RegistrationCouncil { get; set; }

    public int? RegistrationYear { get; set; }

    public string? IdentityProofPath { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal ConsultationFee { get; set; }

    public bool IsVerified { get; set; } = false;

    public bool IsVideoConsult { get; set; } = false;

    public ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();
    public ICollection<DoctorClinic> DoctorClinics { get; set; } = new List<DoctorClinic>();
    public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();
}
