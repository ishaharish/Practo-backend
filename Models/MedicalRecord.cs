using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PractoBackend.Models;

public class MedicalRecord
{
    [Key]
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    [ForeignKey(nameof(AppointmentId))]
    public Appointment Appointment { get; set; } = null!;

    public int PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public PatientProfile Patient { get; set; } = null!;

    public int DoctorId { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public DoctorProfile Doctor { get; set; } = null!;

    [Required]
    public string Diagnosis { get; set; } = string.Empty;

    [Required]
    public string Prescription { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? AttachmentsUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
