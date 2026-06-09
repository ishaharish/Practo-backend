using System.ComponentModel.DataAnnotations;

namespace PractoBackend.DTOs;

public class LabTestBookingDto
{
    [Required]
    public List<int> LabTestIds { get; set; } = new List<int>();
    
    [Required]
    public DateTime BookingDate { get; set; }
    
    public bool HomeCollection { get; set; }
    
    public string Address { get; set; } = string.Empty;
    
    // Patient Details
    [Required(ErrorMessage = "Patient Name is required.")]
    [StringLength(100)]
    public string PatientName { get; set; } = string.Empty;
    
    [Required]
    public int PatientAge { get; set; }

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    // Address Details
    public string Pincode { get; set; } = string.Empty;
    public string HouseOrFlat { get; set; } = string.Empty;
    public string Landmark { get; set; } = string.Empty;

    // Time Slot
    [Required]
    public string TimeSlot { get; set; } = string.Empty;
}
