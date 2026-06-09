using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PractoBackend.DTOs;

public class OnboardMedicalRegDto
{
    [Required]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    public string RegistrationCouncil { get; set; } = string.Empty;

    [Required]
    public int RegistrationYear { get; set; }
}

public class OnboardEstablishmentDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Locality { get; set; } = string.Empty;

    public bool OwnEstablishment { get; set; } = true;
}

public class UploadProofDto
{
    [Required]
    public IFormFile IdentityProof { get; set; } = null!;
}
