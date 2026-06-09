using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractoBackend.Data;
using PractoBackend.Models;

namespace PractoBackend.Controllers;

[ApiController]
[Route("api/doctors/dashboard/patients")]
[AllowAnonymous]
public class DoctorPatientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DoctorPatientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients()
    {
        var patients = await _context.PatientProfiles
            .Include(p => p.User)
            .OrderByDescending(p => p.Id)
            .Select(p => new
            {
                Id = p.Id,
                Name = p.User.FullName,
                Gender = p.Gender.ToString(),
                Mobile = p.User.PhoneNumber,
                Age = p.DateOfBirth.HasValue ? DateTime.Today.Year - p.DateOfBirth.Value.Year : (int?)null,
                LastVisit = _context.Appointments.Where(a => a.PatientId == p.Id).Max(a => (DateTime?)a.BookingDate)
            })
            .ToListAsync();

        return Ok(patients);
    }

    public class CreatePatientDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Gender { get; set; } = "Other";
        public int? Age { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto request)
    {
        var user = new User
        {
            FullName = request.FullName,
            PhoneNumber = request.Mobile,
            Email = $"{request.Mobile}@placeholder.com", // dummy email
            Role = PractoBackend.Enums.Role.Patient
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dob = request.Age.HasValue ? DateTime.Today.AddYears(-request.Age.Value) : DateTime.Today.AddYears(-20);
        
        var genderEnum = Enum.TryParse<PractoBackend.Enums.Gender>(request.Gender, true, out var g) ? g : PractoBackend.Enums.Gender.Other;

        var patient = new PatientProfile
        {
            UserId = user.Id,
            Gender = genderEnum,
            DateOfBirth = dob
        };

        _context.PatientProfiles.Add(patient);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Patient created successfully", PatientId = patient.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatientTimeline(int id)
    {
        var patient = await _context.PatientProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null) return NotFound("Patient not found");

        var appointments = await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Category)
            .Include(a => a.MedicalRecord)
            .Where(a => a.PatientId == id)
            .OrderByDescending(a => a.BookingDate)
            .Select(a => new
            {
                Id = a.Id,
                Date = a.BookingDate,
                DoctorName = a.Doctor != null ? a.Doctor.User.FullName : "Unknown",
                Category = a.Category != null ? a.Category.Name : "Consultation",
                Symptoms = a.Symptoms,
                Status = a.Status.ToString(),
                MedicalRecord = a.MedicalRecord != null ? new
                {
                    Diagnosis = a.MedicalRecord.Diagnosis,
                    Prescription = a.MedicalRecord.Prescription,
                    CreatedAt = a.MedicalRecord.CreatedAt
                } : null
            })
            .ToListAsync();

        return Ok(new
        {
            Id = patient.Id,
            Name = patient.User.FullName,
            Gender = patient.Gender.ToString(),
            BloodGroup = patient.BloodGroup,
            Mobile = patient.User.PhoneNumber,
            Age = patient.DateOfBirth.HasValue ? DateTime.Today.Year - patient.DateOfBirth.Value.Year : (int?)null,
            Timeline = appointments
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var patient = await _context.PatientProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null) return NotFound("Patient not found");

        var appointments = await _context.Appointments.Where(a => a.PatientId == id).ToListAsync();
        var medicalRecords = await _context.MedicalRecords.Where(m => m.PatientId == id).ToListAsync();
        var reviews = await _context.Reviews.Where(r => r.PatientId == id).ToListAsync();

        _context.Reviews.RemoveRange(reviews);
        _context.MedicalRecords.RemoveRange(medicalRecords);
        _context.Appointments.RemoveRange(appointments);
        _context.PatientProfiles.Remove(patient);
        _context.Users.Remove(patient.User);

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Patient and all related records deleted successfully" });
    }
}
