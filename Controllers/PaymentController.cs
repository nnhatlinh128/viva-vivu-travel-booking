using Microsoft.AspNetCore.Mvc;
using ToursAndTravelsManagement.Data;
using ToursAndTravelsManagement.Enums;
using ToursAndTravelsManagement.Services.VNPay;
using ToursAndTravelsManagement.Services.PayPal;

namespace ToursAndTravelsManagement.Controllers;

public class PaymentController : Controller
{
    private readonly VNPayService _vnPayService;
    private readonly PayPalService _payPalService;
    private readonly ApplicationDbContext _context;

    public PaymentController(
        VNPayService vnPayService,
        PayPalService payPalService,
        ApplicationDbContext context)
    {
        _vnPayService = vnPayService;
        _payPalService = payPalService;
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

    [HttpGet]
    public async Task<IActionResult> CreatePayPalPayment(int bookingId)
    {
        try
        {
            var booking = _context.Bookings
                .FirstOrDefault(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return Content("Booking not found");
            }

            var approvalUrl =
                await _payPalService.CreateOrderAsync(
                    booking.BookingId,
                    booking.FinalPrice);

            if (string.IsNullOrEmpty(approvalUrl))
            {
                return Content("Cannot create PayPal order.");
            }

            return Redirect(approvalUrl);
        }
        catch (Exception ex)
        {
            return Content(ex.ToString());
        }
    }

    [HttpGet]
    public async Task<IActionResult> PayPalSuccess(
        string token,
        string PayerID)
    {
        try
        {
            await _payPalService.CaptureOrderAsync(token);

            var booking = _context.Bookings
                .OrderByDescending(x => x.BookingId)
                .FirstOrDefault();

            if (booking == null)
            {
                return Content("Booking not found");
            }

            booking.PaymentStatus = PaymentStatus.Completed;
            booking.Status = BookingStatus.Confirmed;
            booking.PaymentGateway = "PayPal";
            booking.PaymentTransactionId = token;
            booking.PaymentCompletedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return Content("PAYPAL SUCCESS");
        }
        catch (Exception ex)
        {
            return Content(ex.ToString());
        }
    }

    [HttpGet]
    public IActionResult PayPalCancel()
    {
        return RedirectToAction(
            "PaymentFailed",
            "UserBookings");
    }
}