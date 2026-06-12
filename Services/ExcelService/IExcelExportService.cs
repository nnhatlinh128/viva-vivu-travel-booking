using ToursAndTravelsManagement.Models;

namespace ToursAndTravelsManagement.Services.ExcelService;

public interface IExcelExportService
{
    byte[] ExportDestinationsToExcel(List<Destination> destinations);
    byte[] ExportBookingsToExcel(List<Booking> bookings);
    byte[] ExportToursToExcel(List<Tour> tours);
}
