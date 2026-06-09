using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PractoBackend.Data;
using PractoBackend.DTOs;
using PractoBackend.Enums;
using PractoBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace PractoBackend.Controllers;

[ApiController]
[Route("api/consultations")]
public class VideoConsultController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VideoConsultController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("pricing")]
    public IActionResult GetPricing()
    {
        var pricing = new List<VideoConsultPricingDto>
        {
            new VideoConsultPricingDto { Specialty = "Dermatology", Fee = 699 },
            new VideoConsultPricingDto { Specialty = "Sexology", Fee = 749 },
            new VideoConsultPricingDto { Specialty = "Gynaecology", Fee = 749 },
            new VideoConsultPricingDto { Specialty = "General physician", Fee = 699 },
            new VideoConsultPricingDto { Specialty = "Psychiatry", Fee = 899 }
        };

        return Ok(pricing);
    }

    [HttpPost("queue")]
    [Authorize]
    public async Task<IActionResult> JoinQueue([FromBody] VideoConsultQueueDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized("User is not authenticated.");

        var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return BadRequest("Patient profile not found.");

        var appointment = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = null, // Instant queue
            SlotId = null,   // No physical slot
            BookingDate = DateTime.UtcNow,
            Status = AppointmentStatus.WaitingInQueue,
            Symptoms = $"Online Consult - {request.Specialty} (For: {request.PatientName})"
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Simulate payment record (in real app, this happens after gateway confirmation)
        var payment = new Payment
        {
            AppointmentId = appointment.Id,
            Amount = request.AmountPaid,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.UPI,
            Status = PaymentStatus.Successful,
            TransactionId = Guid.NewGuid().ToString()
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Added to queue successfully", AppointmentId = appointment.Id });
    }
}
