using System.ComponentModel.DataAnnotations;

namespace PractoBackend.Models;

public class CalendarCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
