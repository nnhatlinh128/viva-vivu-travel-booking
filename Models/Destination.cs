using System.ComponentModel.DataAnnotations;

namespace ToursAndTravelsManagement.Models;

public class Destination
{
    [Key]
    public int DestinationId { get; set; }

    [Required]
    public string Name { get; set; }

    public string? Description { get; set; } = string.Empty;

    [Required]
    public bool IsDomestic { get; set; }

    [Required]
    public string Country { get; set; }

    [Required]
    public string City { get; set; }

    public string? ImageUrl { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool IsActive { get; set; }
}