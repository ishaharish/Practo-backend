using System.ComponentModel.DataAnnotations;

namespace PractoBackend.DTOs;

public class GenerateSlotsRequestDto
{
    [Required]
    public int ClinicId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    [Range(10, 120)]
    public int SlotDurationMinutes { get; set; }
}
