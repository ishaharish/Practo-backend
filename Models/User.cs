using System.ComponentModel.DataAnnotations;
using PractoBackend.Enums;

namespace PractoBackend.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? PhoneNumber { get; set; }

    public Role Role { get; set; }

    public bool IsActive { get; set; } = true;

    public bool EmailVerified { get; set; } = false;

    [MaxLength(6)]
    public string? OtpCode { get; set; }

    public DateTime? OtpExpiry { get; set; }

    public OtpPurpose? OtpPurpose { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public PatientProfile? PatientProfile { get; set; }
    public DoctorProfile? DoctorProfile { get; set; }
}
