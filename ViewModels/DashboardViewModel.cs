namespace ToursAndTravelsManagement.ViewModels;

public class DashboardViewModel
{
    // KPI cards
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal Profit { get; set; }

    // dùng cho Chart.js
    public List<string> RevenueLabels { get; set; } = new();
    public List<decimal> RevenueValues { get; set; } = new();

    // Charts
    public Dictionary<string, int> BookingStatusData { get; set; } = new();
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
}
