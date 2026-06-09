using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractoBackend.Data;
using PractoBackend.DTOs;
using PractoBackend.Models;

namespace PractoBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SurgeriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SurgeriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSurgeryTypes()
    {
        var types = await _context.SurgeryTypes.ToListAsync();
        return Ok(types);
    }

    [HttpPost("enquiry")]
    public async Task<IActionResult> SubmitEnquiry([FromBody] SurgeryEnquiryDto request)
    {
        var type = await _context.SurgeryTypes.FindAsync(request.SurgeryTypeId);
        if (type == null) return NotFound("Surgery type not found.");

        var enquiry = new SurgeryEnquiry
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            City = request.City,
            SurgeryTypeId = request.SurgeryTypeId,
            CreatedAt = DateTime.UtcNow
        };

        _context.SurgeryEnquiries.Add(enquiry);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Enquiry submitted successfully. We will call you back." });
    }
}
