using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PractoBackend.Data;
using PractoBackend.DTOs;
using PractoBackend.Models;

namespace PractoBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DoctorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetDoctors([FromQuery] string? city, [FromQuery] string? specialization)
    {
        var query = _context.DoctorProfiles
            .Include(dp => dp.User)
            .Include(dp => dp.DoctorSpecializations)
            .ThenInclude(ds => ds.Specialization)
            .Include(dp => dp.DoctorClinics)
            .ThenInclude(dc => dc.Clinic)
            .AsQueryable();

        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(dp => dp.DoctorClinics.Any(dc => dc.Clinic.City == city));
        }

        if (!string.IsNullOrEmpty(specialization))
        {
            query = query.Where(dp => dp.DoctorSpecializations.Any(ds => ds.Specialization.Name == specialization));
        }

        var doctors = await query.Select(dp => new DoctorProfileDto
        {
            Id = dp.Id,
            FullName = dp.User.FullName,
            Email = dp.User.Email,
            ExperienceYears = dp.ExperienceYears,
            Qualifications = dp.Qualifications,
            Biography = dp.Biography,
            ConsultationFee = dp.ConsultationFee,
            IsVerified = dp.IsVerified,
            Specializations = dp.DoctorSpecializations.Select(ds => ds.Specialization.Name).ToList(),
            Clinics = dp.DoctorClinics.Select(dc => new ClinicDto 
            { 
                Id = dc.Clinic.Id, 
                Name = dc.Clinic.Name, 
                City = dc.Clinic.City 
            }).ToList()
        }).ToListAsync();

        return Ok(doctors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDoctor(int id)
    {
        var doctor = await _context.DoctorProfiles
            .Include(dp => dp.User)
            .Where(dp => dp.Id == id)
            .Select(dp => new DoctorProfileDto
            {
                Id = dp.Id,
                FullName = dp.User.FullName,
                Email = dp.User.Email,
                ExperienceYears = dp.ExperienceYears,
                Qualifications = dp.Qualifications,
                Biography = dp.Biography,
                ConsultationFee = dp.ConsultationFee,
                IsVerified = dp.IsVerified,
                Specializations = dp.DoctorSpecializations.Select(ds => ds.Specialization.Name).ToList(),
                Clinics = dp.DoctorClinics.Select(dc => new ClinicDto 
                { 
                    Id = dc.Clinic.Id, 
                    Name = dc.Clinic.Name, 
                    City = dc.Clinic.City 
                }).ToList()
            }).FirstOrDefaultAsync();

        if (doctor == null) return NotFound("Doctor not found.");

        return Ok(doctor);
    }

    [HttpPost("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateDoctorProfileDto request)
    {
        // For now, assume we get the first doctor profile in the database
        var doctorProfile = await _context.DoctorProfiles
            .Include(dp => dp.DoctorClinics)
            .Include(dp => dp.DoctorSpecializations)
            .FirstOrDefaultAsync();
        
        if (doctorProfile == null) return NotFound("Doctor profile not found.");

        doctorProfile.ExperienceYears = request.ExperienceYears;
        doctorProfile.Qualifications = request.Qualifications;
        doctorProfile.Biography = request.Biography;
        doctorProfile.ConsultationFee = request.ConsultationFee;
        doctorProfile.IsVerified = true;

        // Clear and update Clinics association
        _context.DoctorClinics.RemoveRange(doctorProfile.DoctorClinics);
        foreach (var clinicId in request.ClinicIds)
        {
            if (await _context.Clinics.AnyAsync(c => c.Id == clinicId))
            {
                _context.DoctorClinics.Add(new DoctorClinic
                {
                    DoctorId = doctorProfile.Id,
                    ClinicId = clinicId,
                    ConsultationFeeAtClinic = request.ConsultationFee
                });
            }
        }

        // Clear and update Specializations association
        _context.DoctorSpecializations.RemoveRange(doctorProfile.DoctorSpecializations);
        foreach (var specId in request.SpecializationIds)
        {
            if (await _context.Specializations.AnyAsync(s => s.Id == specId))
            {
                _context.DoctorSpecializations.Add(new DoctorSpecialization
                {
                    DoctorId = doctorProfile.Id,
                    SpecializationId = specId
                });
            }
        }

        await _context.SaveChangesAsync();
        return Ok("Profile updated successfully.");
    }

    [HttpPost("slots/generate")]
    public async Task<IActionResult> GenerateSlots([FromBody] GenerateSlotsRequestDto request)
    {
        // TODO: Extract DoctorId from JWT Token
        var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync();
        if (doctorProfile == null) return NotFound("Doctor profile not found.");

        if (request.StartDate > request.EndDate || request.StartTime >= request.EndTime)
            return BadRequest("Invalid date/time range.");

        var slotsToGenerate = new List<AvailabilitySlot>();
        var currentDate = request.StartDate.Date;

        while (currentDate <= request.EndDate.Date)
        {
            var currentTime = request.StartTime;
            while (currentTime.Add(TimeSpan.FromMinutes(request.SlotDurationMinutes)) <= request.EndTime)
            {
                slotsToGenerate.Add(new AvailabilitySlot
                {
                    DoctorId = doctorProfile.Id,
                    ClinicId = request.ClinicId,
                    SlotDate = currentDate,
                    StartTime = currentTime,
                    EndTime = currentTime.Add(TimeSpan.FromMinutes(request.SlotDurationMinutes)),
                    IsBooked = false
                });

                currentTime = currentTime.Add(TimeSpan.FromMinutes(request.SlotDurationMinutes));
            }
            currentDate = currentDate.AddDays(1);
        }

        _context.AvailabilitySlots.AddRange(slotsToGenerate);
        await _context.SaveChangesAsync();
        return Ok($"Successfully generated {slotsToGenerate.Count} slots.");
    }

    [HttpPost("onboard/step1")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> OnboardStep1([FromBody] OnboardMedicalRegDto request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(dp => dp.UserId == userId);
        
        if (doctorProfile == null) return NotFound("Doctor profile not found.");

        doctorProfile.RegistrationNumber = request.RegistrationNumber;
        doctorProfile.RegistrationCouncil = request.RegistrationCouncil;
        doctorProfile.RegistrationYear = request.RegistrationYear;

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Medical registration saved successfully." });
    }

    [HttpPost("onboard/step2")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> OnboardStep2([FromBody] OnboardEstablishmentDto request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(dp => dp.UserId == userId);
        
        if (doctorProfile == null) return NotFound("Doctor profile not found.");

        // Create new clinic
        var clinic = new Clinic
        {
            Name = request.Name,
            City = request.City,
            Address = request.Locality
        };

        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync();

        // Link doctor to clinic
        var doctorClinic = new DoctorClinic
        {
            DoctorId = doctorProfile.Id,
            ClinicId = clinic.Id,
            ConsultationFeeAtClinic = doctorProfile.ConsultationFee
        };

        _context.DoctorClinics.Add(doctorClinic);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Establishment details saved successfully." });
    }

    [HttpPost("onboard/upload-proof")]
    [Authorize(Roles = "Doctor")]
    [RequestSizeLimit(100_000_000)] // 100 MB limit
    public async Task<IActionResult> UploadProof([FromForm] UploadProofDto request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(dp => dp.UserId == userId);
        
        if (doctorProfile == null) return NotFound("Doctor profile not found.");

        if (request.IdentityProof == null || request.IdentityProof.Length == 0)
            return BadRequest("No file uploaded.");

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + "_" + request.IdentityProof.FileName;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.IdentityProof.CopyToAsync(stream);
        }

        doctorProfile.IdentityProofPath = "/uploads/" + uniqueFileName;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Identity proof uploaded successfully.", FilePath = doctorProfile.IdentityProofPath });
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var doctorProfile = await _context.DoctorProfiles
            .Include(dp => dp.User)
            .FirstOrDefaultAsync(dp => dp.UserId == userId);
            
        if (doctorProfile == null) return NotFound("Doctor profile not found.");

        return Ok(new
        {
            Name = doctorProfile.User.FullName,
            Email = doctorProfile.User.Email,
            RegistrationNumber = doctorProfile.RegistrationNumber,
            RegistrationCouncil = doctorProfile.RegistrationCouncil,
            RegistrationYear = doctorProfile.RegistrationYear,
            ConsultationFee = doctorProfile.ConsultationFee,
            IsVerified = doctorProfile.IsVerified
        });
    }

    [HttpGet("dashboard/appointments")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetDashboardAppointments()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(dp => dp.UserId == userId);
        if (doctorProfile == null) return NotFound();

        var appointments = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Slot)
            .Where(a => a.DoctorId == doctorProfile.Id)
            .OrderByDescending(a => a.Slot != null ? a.Slot.SlotDate : a.BookingDate)
            .Select(a => new
            {
                a.Id,
                a.Status,
                a.Symptoms,
                Date = a.Slot != null ? a.Slot.SlotDate : a.BookingDate,
                StartTime = a.Slot != null ? a.Slot.StartTime.ToString(@"hh\:mm") : "",
                PatientName = a.Patient.User.FullName,
                PatientGender = a.Patient.Gender.ToString(),
                PatientDOB = a.Patient.DateOfBirth
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpGet("dashboard/reports")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetDashboardReports()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(dp => dp.UserId == userId);
        if (doctorProfile == null) return NotFound();

        var totalAppointments = await _context.Appointments.CountAsync(a => a.DoctorId == doctorProfile.Id);
        var completedAppointments = await _context.Appointments.CountAsync(a => a.DoctorId == doctorProfile.Id && a.Status == PractoBackend.Enums.AppointmentStatus.Completed);
        var totalRevenue = await _context.Payments
            .Include(p => p.Appointment)
            .Where(p => p.Appointment.DoctorId == doctorProfile.Id && p.Status == PractoBackend.Enums.PaymentStatus.Successful)
            .SumAsync(p => p.Amount);

        return Ok(new
        {
            TotalAppointments = totalAppointments,
            CompletedAppointments = completedAppointments,
            TotalRevenue = totalRevenue
        });
    }
}
