namespace PractoBackend.DTOs;

public class VideoConsultPricingDto
{
    public string Specialty { get; set; } = string.Empty;
    public decimal Fee { get; set; }
}

public class VideoConsultQueueDto
{
    public string Specialty { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientPhoneNumber { get; set; } = string.Empty;
}
