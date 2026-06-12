using ClosedXML.Excel;
using ToursAndTravelsManagement.Models;
using System.Collections.Generic;
using System.IO;

namespace ToursAndTravelsManagement.Services.ExcelService;

public class ExcelExportService : IExcelExportService
{
    public ExcelExportService()
    {
    }

    // Export Destinations to Excel
    public byte[] ExportDestinationsToExcel(List<Destination> destinations)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Destinations");

            // Header row
            worksheet.Cell(1, 1).Value = "Destination ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Country";
            worksheet.Cell(1, 4).Value = "City";
            worksheet.Cell(1, 5).Value = "Is Active";

            // Data rows
            for (int i = 0; i < destinations.Count; i++)
            {
                var destination = destinations[i];
                worksheet.Cell(i + 2, 1).Value = destination.DestinationId;
                worksheet.Cell(i + 2, 2).Value = destination.Name;
                worksheet.Cell(i + 2, 3).Value = destination.Country;
                worksheet.Cell(i + 2, 4).Value = destination.City;
                worksheet.Cell(i + 2, 5).Value = destination.IsActive ? "Yes" : "No";
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }

    // Export Bookings to Excel
    public byte[] ExportBookingsToExcel(List<Booking> bookings)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Bookings");

            // Header row
            worksheet.Cell(1, 1).Value = "Booking ID";
            worksheet.Cell(1, 2).Value = "User";
            worksheet.Cell(1, 3).Value = "Tour";
            worksheet.Cell(1, 4).Value = "Booking Date";
            worksheet.Cell(1, 5).Value = "Participants";
            worksheet.Cell(1, 6).Value = "Total Price";
            worksheet.Cell(1, 7).Value = "Status";

            // Data rows
            for (int i = 0; i < bookings.Count; i++)
            {
                var booking = bookings[i];
                worksheet.Cell(i + 2, 1).Value = booking.BookingId;
                worksheet.Cell(i + 2, 2).Value = booking.User.UserName;
                worksheet.Cell(i + 2, 3).Value = booking.Tour.Name;
                worksheet.Cell(i + 2, 4).Value = booking.BookingDate.ToString("dd/MM/yyyy");
                worksheet.Cell(i + 2, 5).Value = booking.NumberOfParticipants;
                worksheet.Cell(i + 2, 6).Value = booking.TotalPrice;
                worksheet.Cell(i + 2, 7).Value = booking.Status.ToString();
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }

    // Export Tours to Excel
    public byte[] ExportToursToExcel(List<Tour> tours)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Tours");

            // Header row
            worksheet.Cell(1, 1).Value = "Tour ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Destination";
            worksheet.Cell(1, 4).Value = "Price";
            worksheet.Cell(1, 5).Value = "Max Participants";
            worksheet.Cell(1, 6).Value = "Is Active";

            // Data rows
            for (int i = 0; i < tours.Count; i++)
            {
                var tour = tours[i];
                worksheet.Cell(i + 2, 1).Value = tour.TourId;
                worksheet.Cell(i + 2, 2).Value = tour.Name;
                worksheet.Cell(i + 2, 3).Value = tour.Destination.Name;
                worksheet.Cell(i + 2, 4).Value = tour.Price;
                worksheet.Cell(i + 2, 5).Value = tour.MaxParticipants;
                worksheet.Cell(i + 2, 6).Value = tour.IsActive ? "Yes" : "No";
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}
