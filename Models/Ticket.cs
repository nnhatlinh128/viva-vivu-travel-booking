namespace ToursAndTravelsManagement.Models;

public class Ticket
{
    public int TicketId { get; set; }
    public string TicketNumber { get; set; } // Unique ticket number
    public string CustomerName { get; set; }
    public string TourName { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime TourStartDate { get; set; }
    public DateTime TourEndDate { get; set; }
    public decimal TotalPrice { get; set; }
}

