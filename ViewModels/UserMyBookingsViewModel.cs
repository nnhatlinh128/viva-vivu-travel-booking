using ToursAndTravelsManagement.Models;

public class UserMyBookingsViewModel
{
    public ApplicationUser User { get; set; }
    public IEnumerable<Booking> Bookings { get; set; }
}
