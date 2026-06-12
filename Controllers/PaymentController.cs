using Microsoft.AspNetCore.Mvc;
using ToursAndTravelsManagement.Data;
using ToursAndTravelsManagement.Enums;
using ToursAndTravelsManagement.Services.VNPay;

namespace ToursAndTravelsManagement.Controllers;

public class PaymentController : Controller
{
    private readonly VNPayService _vnPayService;
    private readonly ApplicationDbContext _context;

    public PaymentController(
        VNPayService vnPayService,
        ApplicationDbContext context)
    {
        _vnPayService = vnPayService;
        _context = context;
    }

    // =========================
    // CREATE PAYMENT URL
    // =========================
    public IActionResult CreatePayment(int bookingId)
    {
        var booking = _context.Bookings.FirstOrDefault(b =>
            b.BookingId == bookingId);

        if (booking == null)
        {
            return NotFound();
        }

        var paymentUrl = _vnPayService.CreatePaymentUrl(
            HttpContext,
            booking.BookingId,
            booking.FinalPrice);

        return Redirect(paymentUrl);
    }

    // =========================
    // VNPAY RETURN
    // =========================
    public IActionResult VNPayReturn()
    {
        var isValidSignature =
            _vnPayService.ValidateSignature(Request.Query);

        if (!isValidSignature)
        {
            return Content("Invalid signature");
        }

        var responseCode = Request.Query["vnp_ResponseCode"];
        var transactionId = Request.Query["vnp_TransactionNo"];
        var orderInfo = Request.Query["vnp_OrderInfo"];

        // Extract bookingId
        var bookingIdString =
            orderInfo.ToString().Replace("Thanh toan booking ", "");

        if (!int.TryParse(bookingIdString, out int bookingId))
        {
            return Content("Invalid booking id");
        }

        var booking = _context.Bookings.FirstOrDefault(b =>
            b.BookingId == bookingId);

        if (booking == null)
        {
            return NotFound();
        }

        // Payment success
        if (responseCode == "00")
        {
            booking.PaymentStatus = PaymentStatus.Completed;
            booking.Status = BookingStatus.Confirmed;

            booking.PaymentGateway = "VNPay";

            booking.PaymentTransactionId =
                transactionId;

            booking.PaymentCompletedAt =
                DateTime.UtcNow;
        }
        else
        {
            booking.PaymentStatus = PaymentStatus.Failed;

            booking.Status = BookingStatus.Cancelled;

            booking.IsActive = false;
        }

        _context.SaveChanges();

        if (responseCode == "00")
        {
            return RedirectToAction(
                "Success",
                "UserBookings",
                new { bookingId = booking.BookingId });
        }

        return RedirectToAction(
            "PaymentFailed",
            "UserBookings");
    }
}