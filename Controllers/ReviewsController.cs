using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractoBackend.Data;
using PractoBackend.DTOs;
using PractoBackend.Models;

namespace PractoBackend.Controllers;

[ApiController]
[Route("api/appointments/{appointmentId}/review")]
public class ReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReviewsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview(int appointmentId, [FromBody] CreateReviewDto request)
    {
        if (appointmentId != request.AppointmentId) return BadRequest("Appointment ID mismatch.");

        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
        if (appointment == null) return NotFound("Appointment not found.");

        // TODO: Ensure the user is the patient of this appointment
        
        var existingReview = await _context.Reviews.FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);
        if (existingReview != null) return BadRequest("Review already exists for this appointment.");

        if (appointment.DoctorId == null) return BadRequest("Cannot review: no doctor assigned.");

        var review = new Review
        {
            AppointmentId = appointmentId,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId.Value,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Review submitted successfully.", ReviewId = review.Id });
    }
}
