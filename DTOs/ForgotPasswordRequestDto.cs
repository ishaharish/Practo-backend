using System.ComponentModel.DataAnnotations;

namespace PractoBackend.DTOs;

public class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
