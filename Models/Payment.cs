using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PractoBackend.Enums;

namespace PractoBackend.Models;

public class Payment
{
    [Key]
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    [ForeignKey(nameof(AppointmentId))]
    public Appointment Appointment { get; set; } = null!;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public PaymentMethod PaymentMethod { get; set; }

    [MaxLength(255)]
    public string? TransactionId { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
}
