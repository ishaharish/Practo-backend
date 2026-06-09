using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PractoBackend.Models;

public class DoctorClinic
{
    public int DoctorId { get; set; }
    
    [ForeignKey(nameof(DoctorId))]
    public DoctorProfile Doctor { get; set; } = null!;

    public int ClinicId { get; set; }
    
    [ForeignKey(nameof(ClinicId))]
    public Clinic Clinic { get; set; } = null!;

    [Column(TypeName = "decimal(10,2)")]
    public decimal ConsultationFeeAtClinic { get; set; }
}
