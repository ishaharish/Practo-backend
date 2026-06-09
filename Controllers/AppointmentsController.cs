using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractoBackend.Data;
using PractoBackend.DTOs;
using PractoBackend.Enums;
using PractoBackend.Models;

namespace PractoBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("/api/doctors/{doctorId}/slots")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDoctorSlots(int doctorId, [FromQuery] DateTime date)
    {
        var slots = await _context.AvailabilitySlots
            .Where(s => s.DoctorId == doctorId && s.SlotDate == date.Date)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        // Dynamically auto-generate slots if we don't have a full schedule (less than 16 slots for 9 to 5)
        if (slots.Count < 16)
        {
            var doctorClinic = await _context.DoctorClinics.FirstOrDefaultAsync(dc => dc.DoctorId == doctorId);
            int clinicId = doctorClinic?.ClinicId ?? 1; // Default to 1 if not linked

            var startTime = new TimeSpan(9, 0, 0);  // 9:00 AM
            var endTime = new TimeSpan(17, 0, 0); // 5:00 PM
            var slotDuration = TimeSpan.FromMinutes(30);

            var newSlots = new List<AvailabilitySlot>();
            var currentTime = startTime;

            while (currentTime < endTime)
            {
                // Only add if it doesn't already exist
                if (!slots.Any(s => s.StartTime == currentTime))
                {
                    var newSlot = new AvailabilitySlot
                    {
                        DoctorId = doctorId,
                        ClinicId = clinicId,
                        SlotDate = date.Date,
                        StartTime = currentTime,
                        EndTime = currentTime.Add(slotDuration),
                        IsBooked = false
                    };
                    newSlots.Add(newSlot);
                    _context.AvailabilitySlots.Add(newSlot);
                }
                currentTime = currentTime.Add(slotDuration);
            }

            if (newSlots.Any())
            {
                await _context.SaveChangesAsync();
                slots.AddRange(newSlots);
                slots = slots.OrderBy(s => s.StartTime).ToList();
            }
        }

        // IST Timezone conversion to filter past slots
        var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        var istTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

        // If the queried date is today, filter out past slots
        if (date.Date == istTime.Date)
        {
            var currentIstTimeSpan = istTime.TimeOfDay;
            slots = slots.Where(s => s.StartTime > currentIstTimeSpan).ToList();
        }

        // Check against actual appointments (including Walk-ins)
        var appointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.BookingDate.Date == date.Date && a.Status != AppointmentStatus.Cancelled)
            .ToListAsync();

        var result = slots.Select(s => new
        {
            s.Id,
            s.DoctorId,
            s.ClinicId,
            s.SlotDate,
            s.StartTime,
            s.EndTime,
            IsBooked = s.IsBooked || appointments.Any(a => a.BookingDate.TimeOfDay == s.StartTime)
        }).ToList();

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized("User is not authenticated.");

        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return Ok(new List<Appointment>()); // No patient profile means no appointments

        var appointments = await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.DoctorClinics).ThenInclude(dc => dc.Clinic)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Slot)
            .Where(a => a.PatientId == patient.Id)
            .OrderByDescending(a => a.Slot != null ? a.Slot.SlotDate : a.BookingDate)
            .ToListAsync();

        var dtos = appointments.Select(a => new
        {
            Id = a.Id,
            DoctorName = a.Doctor?.User?.FullName,
            ClinicName = (a.Doctor != null && a.Doctor.DoctorClinics != null) 
                            ? a.Doctor.DoctorClinics.FirstOrDefault()?.Clinic?.Name ?? "Practo Clinic" 
                            : "Practo Clinic",
            SlotDate = a.Slot != null ? a.Slot.SlotDate : a.BookingDate,
            StartTime = a.Slot != null ? a.Slot.StartTime.ToString() : a.BookingDate.TimeOfDay.ToString(),
            Status = a.Status.ToString(),
            Symptoms = a.Symptoms
        });

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequestDto request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized("User is not authenticated.");

        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null)
        {
            patient = new PatientProfile 
            { 
                UserId = userId, 
                Gender = Gender.Other, 
                DateOfBirth = DateTime.Today.AddYears(-20) 
            };
            _context.PatientProfiles.Add(patient);
            await _context.SaveChangesAsync();
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var slot = await _context.AvailabilitySlots.FirstOrDefaultAsync(s => s.Id == request.SlotId);
            if (slot == null || slot.DoctorId != request.DoctorId)
                return NotFound("Slot not found.");

            if (slot.IsBooked)
                return BadRequest("Slot is already booked.");

            // Mark slot as booked
            slot.IsBooked = true;
            // Update happens due to EF tracking. Concurrency exception might be thrown here if 2 users book simultaneously.

            var appointment = new Appointment
            {
                PatientId = patient.Id,
                DoctorId = request.DoctorId,
                SlotId = request.SlotId,
                Symptoms = request.Symptoms,
                Status = AppointmentStatus.Pending,
                BookingDate = slot.SlotDate.Date + slot.StartTime,
                DurationMinutes = 30, // Default duration for standard slots
                QueueStatus = AppointmentQueueStatus.Scheduled,
                TokenNumber = $"TKN-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}"
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Fetch Doctor's Consultation Fee
            var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.Id == request.DoctorId);

            // Create Pending Payment Record
            var payment = new Payment
            {
                AppointmentId = appointment.Id,
                Amount = doctor?.ConsultationFee ?? 0,
                Currency = "USD",
                PaymentMethod = PaymentMethod.CreditCard, // Defaulting for now
                Status = PaymentStatus.Pending
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new { Message = "Appointment booked successfully.", AppointmentId = appointment.Id });
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return Conflict("The slot was booked by someone else. Please choose another slot.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An error occurred while booking the appointment.");
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto request)
    {
        // TODO: Ensure the user updating is the corresponding Doctor
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (appointment == null) return NotFound("Appointment not found.");

        appointment.Status = request.Status;
        await _context.SaveChangesAsync();

        return Ok($"Appointment status updated to {request.Status}");
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelAppointment(int id, [FromBody] CancelAppointmentDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Slot)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound("Appointment not found.");

            if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Completed)
                return BadRequest("Cannot cancel this appointment.");

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancellationReason = request.Reason;

            // Free the slot
            if (appointment.Slot != null)
            {
                appointment.Slot.IsBooked = false;
            }

            // Also handle payment cancellation if necessary
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.AppointmentId == id);
            if (payment != null && payment.Status == PaymentStatus.Pending)
            {
                payment.Status = PaymentStatus.Failed; // Or some cancelled enum
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("Appointment cancelled successfully.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An error occurred while cancelling the appointment.");
        }
    }
}
