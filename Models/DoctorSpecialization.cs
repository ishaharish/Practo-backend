using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PractoBackend.Models;

public class DoctorSpecialization
{
    public int DoctorId { get; set; }
    
    [ForeignKey(nameof(DoctorId))]
    public DoctorProfile Doctor { get; set; } = null!;

    public int SpecializationId { get; set; }
    
    [ForeignKey(nameof(SpecializationId))]
    public Specialization Specialization { get; set; } = null!;
}
