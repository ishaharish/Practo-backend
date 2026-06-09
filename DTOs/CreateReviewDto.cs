using System.ComponentModel.DataAnnotations;

namespace PractoBackend.DTOs;

public class CreateReviewDto
{
    [Required]
    public int AppointmentId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Comment { get; set; }
}
