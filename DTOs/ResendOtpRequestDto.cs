using System.ComponentModel.DataAnnotations;

namespace PractoBackend.DTOs;

public class ResendOtpRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
