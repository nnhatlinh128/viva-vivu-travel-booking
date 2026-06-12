using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ToursAndTravelsManagement.Attributes;

namespace ToursAndTravelsManagement.Models;
public class Tour
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TourId { get; set; }

    [Required]
    public string Name { get; set; }

    public string? Description { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    [FutureDate]
    public DateTime EndDate { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int MaxParticipants { get; set; }

    [Required]
    public int DestinationId { get; set; }

    [ForeignKey("DestinationId")]
    public Destination? Destination { get; set; }

    public string? CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public bool IsActive { get; set; }
}