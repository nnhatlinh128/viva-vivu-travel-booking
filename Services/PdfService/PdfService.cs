using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ToursAndTravelsManagement.Models;

namespace ToursAndTravelsManagement.Services.PdfService
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateToursPdf(string title, string content)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header()
                        .Text(title)
                        .FontSize(24)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .PaddingVertical(10)
                        .Text(content)
                        .FontSize(12);

                    page.Footer()
                        .AlignRight()
                        .Text($"Generated on {DateTime.UtcNow.ToString("dd MMM yyyy")}")
                        .FontSize(10);
                });
            });

            // Generate and return the PDF as a byte array
            return document.GeneratePdf();
        }

        public byte[] GenerateDestinationsPdf(List<Destination> destinations)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Destinations Report").FontSize(20).Bold();
                    page.Content().Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Table headers
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Destination");
                            header.Cell().Element(CellStyle).Text("Country");
                            header.Cell().Element(CellStyle).Text("City");
                            header.Cell().Element(CellStyle).Text("Is Active");
                        });

                        // Table data (rows)
                        foreach (var destination in destinations)
                        {
                            table.Cell().Element(CellStyle).Text(destination.Name);
                            table.Cell().Element(CellStyle).Text(destination.Country);
                            table.Cell().Element(CellStyle).Text(destination.City);
                            table.Cell().Element(CellStyle).Text(destination.IsActive ? "Yes" : "No");
                        }
                    });

                    // Cell styling
                    static IContainer CellStyle(IContainer container) => container.Padding(5);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateBookingsPdf(List<Booking> bookings)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Bookings Report").FontSize(20).Bold();
                    page.Content().Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(); // User
                            columns.RelativeColumn(); // Tour
                            columns.RelativeColumn(); // Booking Date
                            columns.RelativeColumn(); // Participants
                            columns.RelativeColumn(); // Total Price
                            columns.RelativeColumn(); // Status
                        });

                        // Table headers
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("User");
                            header.Cell().Element(CellStyle).Text("Tour");
                            header.Cell().Element(CellStyle).Text("Booking Date");
                            header.Cell().Element(CellStyle).Text("Participants");
                            header.Cell().Element(CellStyle).Text("Total Price");
                            header.Cell().Element(CellStyle).Text("Status");
                        });

                        // Table data (rows)
                        foreach (var booking in bookings)
                        {
                            table.Cell().Element(CellStyle).Text(booking.User.UserName);
                            table.Cell().Element(CellStyle).Text(booking.Tour.Name);
                            table.Cell().Element(CellStyle).Text(booking.BookingDate.ToString("dd/MM/yyyy"));
                            table.Cell().Element(CellStyle).Text(booking.NumberOfParticipants.ToString());
                            table.Cell().Element(CellStyle).Text(booking.TotalPrice.ToString("C"));
                            table.Cell().Element(CellStyle).Text(booking.Status);
                        }
                    });

                    // Cell styling
                    static IContainer CellStyle(IContainer container) => container.Padding(5);
                });
            });

            return document.GeneratePdf();
        }
        public byte[] GenerateTicketPdf(Ticket ticket)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text("Tours and Travels Management").FontSize(24).Bold().FontColor(Colors.Blue.Medium);
                            column.Item().Padding(10).Text($"Ticket Number: {ticket.TicketNumber}").FontSize(16).Bold(); // Uniform padding
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(10)
                        .Column(column =>
                        {
                            column.Item().Padding(5).Text($"Customer Name: {ticket.CustomerName}").Bold();
                            column.Item().Padding(5).Text($"Tour Name: {ticket.TourName}");
                            column.Item().Padding(5).Text($"Booking Date: {ticket.BookingDate:dd/MM/yyyy}");
                            column.Item().Padding(5).Text($"Tour Start Date: {ticket.TourStartDate:dd/MM/yyyy}");
                            column.Item().Padding(5).Text($"Tour End Date: {ticket.TourEndDate:dd/MM/yyyy}");
                            column.Item().Padding(5).Text($"Total Price: {ticket.TotalPrice:C}");
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Generated on: ").FontColor(Colors.Grey.Medium);
                            text.Span($"{DateTime.UtcNow:dd/MM/yyyy HH:mm}");
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
