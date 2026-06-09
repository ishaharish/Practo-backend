using System.ComponentModel.DataAnnotations;

namespace PractoBackend.Models;

public class LabTestBooking
{
    public int Id { get; set; }
    
    public int UserId { get; set; } // The account that paid
    
    public User? User { get; set; }
    
    public int LabTestId { get; set; }
    
    public LabTest? LabTest { get; set; }
    
    public DateTime BookingDate { get; set; }
    
    [StringLength(50)]
    public string Status { get; set; } = "Pending";
    
    public bool HomeCollection { get; set; }
    
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
    
    // Patient Details
    [StringLength(100)]
    public string PatientName { get; set; } = string.Empty; 
    
    public int PatientAge { get; set; }

    [StringLength(20)]
    public string Gender { get; set; } = string.Empty;

    [StringLength(20)]
    public string MobileNumber { get; set; } = string.Empty;

    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    // Address Details
    [StringLength(20)]
    public string Pincode { get; set; } = string.Empty;

    [StringLength(200)]
    public string HouseOrFlat { get; set; } = string.Empty;

    [StringLength(200)]
    public string Landmark { get; set; } = string.Empty;

    // Time Slot
    [StringLength(100)]
    public string TimeSlot { get; set; } = string.Empty;
}
