using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractoBackend.Data;
using PractoBackend.DTOs;
using PractoBackend.Models;
using PractoBackend.Enums;

namespace PractoBackend.Controllers;

[ApiController]
[Route("api/ray")]
[AllowAnonymous]
public class RayCalendarController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RayCalendarController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendar([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? specificDoctorId)
    {
        // Enforce IST internally or assume the input is correct. For this implementation, we will query based on the UTC equivalent of the required IST timeframe.
        // We will just query by the provided dates. The frontend should send UTC dates that correspond to the start/end of the IST day.
        
        var query = _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Category)
            .Where(a => a.BookingDate >= startDate && a.BookingDate <= endDate);

        if (specificDoctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == specificDoctorId.Value);
        }

        var appointments = await query.Distinct().ToListAsync();

        var dtos = appointments.Select(a => new RayAppointmentDto
        {
            Id = a.Id,
            PatientName = a.Patient?.User?.FullName ?? "Walk-in Patient",
            PatientMobile = a.Patient?.User?.PhoneNumber ?? "",
            AbhaId = a.AbhaId,
            TokenNumber = a.TokenNumber,
            BookingDate = a.BookingDate,
            DurationMinutes = a.DurationMinutes,
            CategoryId = a.CategoryId,
            CategoryName = a.Category?.Name,
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor?.User?.FullName ?? "Unknown Doctor",
            PlannedProcedures = a.PlannedProcedures,
            InternalNotes = a.InternalNotes,
            QueueStatus = a.QueueStatus,
            NotifyPatientSms = a.NotifyPatientSms,
            NotifyPatientEmail = a.NotifyPatientEmail
        }).ToList();

        return Ok(dtos);
    }

    [HttpPost("appointments/walk-in")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateWalkIn([FromBody] WalkInAppointmentDto request)
    {
        // Check if user exists by Phone Number first
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PatientMobile);
        
        // If not found by phone, check by Email to prevent unique constraint violation
        if (user == null && !string.IsNullOrEmpty(request.PatientEmail))
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.PatientEmail);
        }

        PatientProfile patient;

        if (user == null)
        {
            user = new User
            {
                FullName = request.PatientName,
                PhoneNumber = request.PatientMobile,
                Email = string.IsNullOrWhiteSpace(request.PatientEmail) ? $"{request.PatientMobile}@walkin.local" : request.PatientEmail,
                Role = Role.Patient,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            patient = new PatientProfile { UserId = user.Id };
            _context.PatientProfiles.Add(patient);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Sync the user's name if they walked in with a different registered name
            if (user.FullName != request.PatientName)
            {
                user.FullName = request.PatientName;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient == null)
            {
                patient = new PatientProfile { UserId = user.Id };
                _context.PatientProfiles.Add(patient);
                await _context.SaveChangesAsync();
            }
        }

        // Prevent multiple walk-in records for the same patient on the same day. 
        // If they book again, simply update their existing appointment time/doctor for today.
        var existingAppt = await _context.Appointments.FirstOrDefaultAsync(a => 
            a.PatientId == patient.Id && 
            a.BookingDate.Date == request.BookingDate.Date);

        if (existingAppt != null)
        {
            existingAppt.BookingDate = request.BookingDate;
            existingAppt.DoctorId = request.DoctorId;
            existingAppt.DurationMinutes = request.DurationMinutes;
            existingAppt.CategoryId = request.CategoryId;
            existingAppt.PlannedProcedures = request.PlannedProcedures;
            existingAppt.InternalNotes = request.InternalNotes;
            
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Updated existing appointment for today.", Token = existingAppt.TokenNumber, AppointmentId = existingAppt.Id });
        }

        var token = $"TKN-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        var appointment = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = request.DoctorId,
            BookingDate = request.BookingDate,
            DurationMinutes = request.DurationMinutes,
            CategoryId = request.CategoryId,
            PlannedProcedures = request.PlannedProcedures,
            InternalNotes = request.InternalNotes,
            QueueStatus = AppointmentQueueStatus.Scheduled,
            NotifyPatientSms = request.NotifyPatientSms,
            NotifyPatientEmail = request.NotifyPatientEmail,
            AbhaId = request.AbhaId,
            TokenNumber = token,
            Status = AppointmentStatus.Confirmed
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Walk-in appointment created successfully", Token = token, AppointmentId = appointment.Id });
    }

    [HttpPut("appointments/{id}/reschedule")]
    public async Task<IActionResult> RescheduleAppointment(int id, [FromBody] RescheduleAppointmentDto request)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(a => a.Id == id);
            
        if (appointment == null) return NotFound("Appointment not found");

        var newDateTime = request.NewDate.Date + request.NewStartTime;
        appointment.BookingDate = newDateTime;

        await _context.SaveChangesAsync();

        if (request.NotifyPatient && !string.IsNullOrEmpty(appointment.Patient?.User?.Email))
        {
            var emailService = HttpContext.RequestServices.GetService<PractoBackend.Services.IEmailService>();
            if (emailService != null)
            {
                var body = $"<p>Dear {appointment.Patient.User.FullName},</p><p>Your appointment has been successfully rescheduled to <b>{newDateTime:f}</b>.</p>";
                await emailService.SendEmailAsync(appointment.Patient.User.Email, "Appointment Rescheduled", body);
            }
        }

        return Ok(new { Message = "Appointment rescheduled successfully", NewBookingDate = newDateTime });
    }

    [HttpDelete("appointments/{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound("Appointment not found");

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Appointment deleted successfully" });
    }

    [HttpPut("appointments/{id}/mark-done")]
    public async Task<IActionResult> MarkAppointmentDone(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound("Appointment not found");

        appointment.QueueStatus = PractoBackend.Enums.AppointmentQueueStatus.Done;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Appointment marked as done" });
    }

    [HttpGet("reports")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReports()
    {
        var appointments = await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .ToListAsync();
        
        var totalAppointments = appointments.Count;
        var completedAppointments = appointments.Where(a => a.QueueStatus == PractoBackend.Enums.AppointmentQueueStatus.Done).ToList();
        
        // Revenue logic: sum the consultation fee for completed appointments (default to 500 if unassigned)
        var totalRevenue = completedAppointments.Sum(a => a.Doctor?.ConsultationFee ?? 500);

        var recentRevenue = completedAppointments.Select(a => new
        {
            PatientName = a.Patient?.User?.FullName ?? "Unknown",
            DoctorName = a.Doctor?.User?.FullName ?? "Unknown",
            Age = a.Patient?.DateOfBirth.HasValue == true ? DateTime.Today.Year - a.Patient.DateOfBirth.Value.Year : (int?)null,
            ConsultationFee = a.Doctor?.ConsultationFee ?? 500,
            Date = a.BookingDate
        }).OrderByDescending(a => a.Date).ToList();

        return Ok(new
        {
            TotalAppointments = totalAppointments,
            CompletedAppointments = completedAppointments.Count,
            TotalRevenue = totalRevenue,
            RevenueDetails = recentRevenue
        });
    }

    [HttpPost("appointments/next-visit")]
    public async Task<IActionResult> CreateNextVisit([FromBody] NextVisitDto request)
    {
        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.Id == request.PatientId);
        if (patient == null) return NotFound("Patient not found.");

        // We assume the logged in doctor is booking this, but for prototype we just grab any default doctor if none passed.
        int doctorId = request.DoctorId ?? (await _context.DoctorProfiles.FirstOrDefaultAsync())?.Id ?? 1;

        var token = $"TKN-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            DoctorId = doctorId,
            BookingDate = request.NextVisitDate,
            DurationMinutes = 30, // Default duration
            QueueStatus = AppointmentQueueStatus.Scheduled,
            Status = AppointmentStatus.Confirmed,
            TokenNumber = token
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Next visit scheduled successfully.", AppointmentId = appointment.Id });
    }
}
