using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursAndTravelsManagement.Repositories.IRepositories;
using ToursAndTravelsManagement.ViewModels;
using ToursAndTravelsManagement.Enums;

[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var bookings = await _unitOfWork.BookingRepository.GetAllAsync();

        var paidBookings = bookings
            .Where(b => b.PaymentStatus == PaymentStatus.Completed)
            .ToList();

        var model = new DashboardViewModel
        {
            // KPI
            TotalBookings = bookings.Count(),
            CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
            TotalRevenue = paidBookings.Sum(b => b.TotalPrice),
            Profit = paidBookings.Sum(b => b.TotalPrice) * 0.2m, // giả định 20%

            // Charts
            BookingStatusData = bookings
                .GroupBy(b => b.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),

            RevenueByMonth = paidBookings
                .GroupBy(b => b.BookingDate.ToString("MM/yyyy"))
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(b => b.TotalPrice)
                )
        };

        return View(model);
    }
}
