using System.ComponentModel.DataAnnotations;
using PractoBackend.Enums;

namespace PractoBackend.DTOs;

public class RayAppointmentDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientMobile { get; set; } = string.Empty;
    public string? AbhaId { get; set; }
    public string? TokenNumber { get; set; }
    public DateTime BookingDate { get; set; }
    public int DurationMinutes { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string? PlannedProcedures { get; set; }
    public string? InternalNotes { get; set; }
    public AppointmentQueueStatus QueueStatus { get; set; }
    public bool NotifyPatientSms { get; set; }
    public bool NotifyPatientEmail { get; set; }
}

public class WalkInAppointmentDto
{
    [Required]
    public string PatientName { get; set; } = string.Empty;
    
    [Required]
    public string PatientMobile { get; set; } = string.Empty;
    
    public string? PatientEmail { get; set; }
    
    public string? AbhaId { get; set; }
    
    [Required]
    public int DoctorId { get; set; }
    
    public int? CategoryId { get; set; }
    
    [Required]
    public DateTime BookingDate { get; set; }
    
    public int DurationMinutes { get; set; } = 30;
    
    public string? PlannedProcedures { get; set; }
    
    public string? InternalNotes { get; set; }
    
    public bool NotifyPatientSms { get; set; }
    
    public bool NotifyPatientEmail { get; set; }
}

public class RescheduleAppointmentDto
{
    [Required]
    public DateTime NewDate { get; set; }
    
    [Required]
    public TimeSpan NewStartTime { get; set; }
    
    public bool NotifyPatient { get; set; }
    
    public bool NotifyDoctor { get; set; }
}

public class NextVisitDto
{
    public int PatientId { get; set; }
    public DateTime NextVisitDate { get; set; }
    public int? DoctorId { get; set; }
}
