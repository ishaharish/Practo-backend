using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractoBackend.Data;
using PractoBackend.DTOs;
using PractoBackend.Models;
using System.Security.Claims;

namespace PractoBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LabTestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LabTestsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetLabTests()
    {
        var tests = await _context.LabTests.ToListAsync();
        return Ok(tests);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLabTest(int id)
    {
        var test = await _context.LabTests.FindAsync(id);
        if (test == null) return NotFound();
        return Ok(test);
    }

    [Authorize]
    [HttpPost("book")]
    public async Task<IActionResult> BookLabTest([FromBody] LabTestBookingDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized("User not authenticated.");
        int userId = int.Parse(userIdClaim.Value);

        if (request.LabTestIds == null || !request.LabTestIds.Any())
            return BadRequest("No lab tests specified.");

        var bookings = new List<LabTestBooking>();

        foreach (var testId in request.LabTestIds)
        {
            var test = await _context.LabTests.FindAsync(testId);
            if (test == null) continue;

            var booking = new LabTestBooking
            {
                UserId = userId,
                LabTestId = testId,
                BookingDate = request.BookingDate,
                HomeCollection = request.HomeCollection,
                Address = request.Address,
                PatientName = request.PatientName,
                PatientAge = request.PatientAge,
                Gender = request.Gender,
                MobileNumber = request.MobileNumber,
                Email = request.Email,
                Pincode = request.Pincode,
                HouseOrFlat = request.HouseOrFlat,
                Landmark = request.Landmark,
                TimeSlot = request.TimeSlot,
                Status = "Pending"
            };
            
            bookings.Add(booking);
        }

        if (!bookings.Any())
            return NotFound("None of the specified lab tests were found.");

        _context.LabTestBookings.AddRange(bookings);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Lab tests booked successfully", BookingIds = bookings.Select(b => b.Id) });
    }
}
