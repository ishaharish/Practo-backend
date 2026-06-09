using System.ComponentModel.DataAnnotations;
using PractoBackend.Enums;

namespace PractoBackend.DTOs;

public class UpdateAppointmentStatusDto
{
    [Required]
    public AppointmentStatus Status { get; set; }
}
