using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Enums;

namespace ToursAndTravelsManagement.Data;

public static class DashboardSeeder
{
    public static void SeedBookings(ApplicationDbContext context)
    {
        // ❗ Nếu đã có booking thì KHÔNG seed nữa
        if (context.Bookings.Any())
            return;

        // ❗ LẤY TOUR THẬT TRONG DB
        var tours = context.Tours.Take(3).ToList();

        if (!tours.Any())
        {
            // Không có tour thì không seed booking
            return;
        }

        var bookings = new List<Booking>();

        var random = new Random();

        foreach (var tour in tours)
        {
            for (int i = 0; i < 4; i++)
            {
                bookings.Add(new Booking
                {
                    TourId = tour.TourId, // ✅ TOUR TỒN TẠI
                    BookingDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                    NumberOfParticipants = random.Next(1, 5),
                    TotalPrice = random.Next(2_000_000, 8_000_000),
                    Status = BookingStatus.Confirmed,
                    PaymentMethod = PaymentMethod.VNPay,
                    PaymentStatus = PaymentStatus.Completed, // ⚠️ dùng đúng enum của bạn
                    CreatedBy = "DashboardSeeder",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                });
            }
        }

        context.Bookings.AddRange(bookings);
        context.SaveChanges();
    }
}
