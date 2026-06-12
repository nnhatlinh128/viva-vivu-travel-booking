using Microsoft.AspNetCore.Identity;
using ToursAndTravelsManagement.Enums;


namespace ToursAndTravelsManagement.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; } = string.Empty;
    public string? City { get; set; } = string.Empty;
    public string? Country { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public decimal TotalRevenue { get; set; } = 0;
    public int? MembershipTierId { get; set; }
    public MembershipTier MembershipTier { get; set; }
}

public class Bookings
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public int TourId { get; set; }
    public Tour Tour { get; set; }

    public DateTime StartDate { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal TotalPrice { get; set; }

    public BookingStatus Status { get; set; }
}


