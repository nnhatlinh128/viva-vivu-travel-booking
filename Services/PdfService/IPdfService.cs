using ToursAndTravelsManagement.Models;

namespace ToursAndTravelsManagement.Services.PdfService
{
    public interface IPdfService
    {
        byte[] GenerateToursPdf(string title, string content);
        byte[] GenerateDestinationsPdf(List<Destination> destinations);
        byte[] GenerateBookingsPdf(List<Booking> bookings);
        byte[] GenerateTicketPdf(Ticket ticket);
    }
}
