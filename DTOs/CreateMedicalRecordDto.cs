using System.ComponentModel.DataAnnotations;

namespace PractoBackend.DTOs;

public class CreateMedicalRecordDto
{
    [Required]
    public int AppointmentId { get; set; }

    [Required]
    public string Diagnosis { get; set; } = string.Empty;

    [Required]
    public string Prescription { get; set; } = string.Empty;

    public string? AttachmentsUrl { get; set; }
}
