using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PractoBackend.Enums;

namespace PractoBackend.Models;

public class Appointment
{
    [Key]
    public int Id { get; set; }

    public int PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public PatientProfile Patient { get; set; } = null!;

    public int? DoctorId { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public DoctorProfile? Doctor { get; set; }

    public int? SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public AvailabilitySlot? Slot { get; set; }

    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    public string? Symptoms { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    [MaxLength(255)]
    public string? CancellationReason { get; set; }

    [MaxLength(50)]
    public string? AbhaId { get; set; }

    [MaxLength(20)]
    public string? TokenNumber { get; set; }

    public int DurationMinutes { get; set; } = 30;

    public int? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public CalendarCategory? Category { get; set; }

    [MaxLength(255)]
    public string? PlannedProcedures { get; set; }

    public string? InternalNotes { get; set; }

    public AppointmentQueueStatus QueueStatus { get; set; } = AppointmentQueueStatus.Scheduled;

    public bool NotifyPatientSms { get; set; } = false;

    public bool NotifyPatientEmail { get; set; } = false;

    public MedicalRecord? MedicalRecord { get; set; }
    public Review? Review { get; set; }
    public Payment? Payment { get; set; }
}
