using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PractoBackend.Data;
using PractoBackend.DTOs;
using PractoBackend.Models;

namespace PractoBackend.Controllers;

[ApiController]
[Route("api/appointments/{appointmentId}/medical-record")]
[AllowAnonymous]
public class MedicalRecordsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MedicalRecordsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMedicalRecord(int appointmentId, [FromBody] CreateMedicalRecordDto request)
    {
        if (appointmentId != request.AppointmentId) return BadRequest("Appointment ID mismatch.");

        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
        if (appointment == null) return NotFound("Appointment not found.");

        // TODO: Ensure the user is the doctor of this appointment
        
        var existingRecord = await _context.MedicalRecords.FirstOrDefaultAsync(m => m.AppointmentId == appointmentId);
        if (existingRecord != null) return BadRequest("Medical record already exists for this appointment.");

        if (appointment.DoctorId == null) return BadRequest("Cannot add record: no doctor assigned.");

        var medicalRecord = new MedicalRecord
        {
            AppointmentId = appointmentId,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId.Value,
            Diagnosis = request.Diagnosis,
            Prescription = request.Prescription,
            AttachmentsUrl = request.AttachmentsUrl
        };

        _context.MedicalRecords.Add(medicalRecord);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Medical record created successfully.", RecordId = medicalRecord.Id });
    }
}
