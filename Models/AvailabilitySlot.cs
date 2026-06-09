using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PractoBackend.Models;

public class AvailabilitySlot
{
    [Key]
    public int Id { get; set; }

    public int DoctorId { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public DoctorProfile Doctor { get; set; } = null!;

    public int ClinicId { get; set; }

    [ForeignKey(nameof(ClinicId))]
    public Clinic Clinic { get; set; } = null!;

    [Required]
    public DateTime SlotDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public bool IsBooked { get; set; } = false;

    // Fluent API will map this to a concurrency token
    public byte[] RowVersion { get; set; } = null!;
}
