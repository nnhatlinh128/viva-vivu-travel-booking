using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ToursAndTravelsManagement.Enums;

namespace ToursAndTravelsManagement.Models;
public class Booking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BookingId { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    [Required]
    public int TourId { get; set; }
    public Tour? Tour { get; set; }

    [Required]
    public DateTime BookingDate { get; set; }

    [Required]
    public int NumberOfParticipants { get; set; }

    [Required]
    public decimal TotalPrice { get; set; }

    [Required]
    public BookingStatus Status { get; set; }

    [Required(ErrorMessage = "Payment Method is required")]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    public PaymentStatus PaymentStatus { get; set; }

    public string? CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public bool IsActive { get; set; }

    public int? VoucherId { get; set; }
    public Voucher? Voucher { get; set; }

    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }

    public string? BookingCode { get; set; }

    public string? PaymentTransactionId { get; set; }

    public string? PaymentGateway { get; set; }

    public DateTime? PaymentCompletedAt { get; set; }
}