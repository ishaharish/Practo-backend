using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PractoBackend.Enums;

namespace PractoBackend.Models;

public class PatientProfile
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }

    public Gender? Gender { get; set; }

    [MaxLength(5)]
    public string? BloodGroup { get; set; }

    [MaxLength(15)]
    public string? EmergencyContact { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
