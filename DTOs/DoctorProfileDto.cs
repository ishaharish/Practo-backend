using System.ComponentModel.DataAnnotations;

namespace PractoBackend.DTOs;

public class DoctorProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string Qualifications { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public decimal ConsultationFee { get; set; }
    public bool IsVerified { get; set; }
    public List<string> Specializations { get; set; } = new List<string>();
    public List<ClinicDto> Clinics { get; set; } = new List<ClinicDto>();
}

public class ClinicDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}
