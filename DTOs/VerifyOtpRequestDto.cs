using System.ComponentModel.DataAnnotations;

namespace PractoBackend.DTOs;

public class VerifyOtpRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(6)]
    public string OtpCode { get; set; } = string.Empty;
}
