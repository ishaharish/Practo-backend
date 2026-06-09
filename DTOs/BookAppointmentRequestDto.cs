using System.ComponentModel.DataAnnotations;

namespace PractoBackend.DTOs;

public class BookAppointmentRequestDto
{
    [Required]
    public int DoctorId { get; set; }

    [Required]
    public int SlotId { get; set; }

    public string? Symptoms { get; set; }
}
