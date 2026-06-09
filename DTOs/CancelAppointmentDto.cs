using System.ComponentModel.DataAnnotations;

namespace PractoBackend.DTOs;

public class CancelAppointmentDto
{
    [Required]
    [MaxLength(255)]
    public string Reason { get; set; } = string.Empty;
}
