using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories.IRepositories;
using ToursAndTravelsManagement.Services.ExcelService;
using ToursAndTravelsManagement.Services.PdfService;

namespace ToursAndTravelsManagement.Controllers;

[Authorize(Policy = "RequireAdminRole")]
public class BookingsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfService _pdfService;
    private readonly IExcelExportService _excelExportService;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IPdfService pdfService, IExcelExportService excelExportService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _pdfService = pdfService;
        _excelExportService = excelExportService;
    }

    // Export PDF Action
    [HttpGet]
    public async Task<IActionResult> ExportPdf()
    {
        var bookings = await _unitOfWork.BookingRepository.GetAllAsync(includeProperties: "User,Tour");
        if (bookings == null || !bookings.Any())
        {
            return NotFound("No bookings found.");
        }

        // Generate the PDF
        var pdfContent = _pdfService.GenerateBookingsPdf(bookings.ToList());

        // Return the PDF as a file download
        return File(pdfContent, "application/pdf", "BookingsReport.pdf");
    }

    // Export Excel Action
    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        var bookings = await _unitOfWork.BookingRepository.GetAllAsync(includeProperties: "User,Tour");
        var excelContent = _excelExportService.ExportBookingsToExcel(bookings.ToList());
        return File(excelContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Bookings.xlsx");
    }

    // GET: Bookings
    public async Task<IActionResult> Index(int? pageNumber, int? pageSize)
    {
        Log.Information("Fetching bookings with pageNumber: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);

        int pageIndex = pageNumber ?? 1;
        int size = pageSize ?? 10;

        var bookings = await _unitOfWork.BookingRepository.GetPaginatedAsync(pageIndex, size, "User,Tour");
        return View(bookings);
    }

    // GET: Bookings/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            Log.Warning("Details action called with null id");
            return NotFound();
        }

        Log.Information("Fetching details for booking with id: {Id}", id);

        var booking = await _unitOfWork.BookingRepository.GetByIdAsync(id.Value, "User,Tour");
        if (booking == null)
        {
            Log.Warning("Booking with id {Id} not found", id);
            return NotFound();
        }

        return View(booking);
    }

    // GET: Bookings/Create
    public async Task<IActionResult> Create()
    {
        Log.Information("Creating a new booking");

        var users = await _userManager.Users.ToListAsync();
        ViewBag.UserId = new SelectList(users, "Id", "UserName");
        ViewBag.TourId = new SelectList(await _unitOfWork.TourRepository.GetAllAsync(), "TourId", "Name");
        return View();
    }

    // POST: Bookings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UserId,TourId,BookingDate,NumberOfParticipants,TotalPrice,Status,PaymentMethod,PaymentStatus,IsActive")] Booking booking)
    {
        if (ModelState.IsValid)
        {
            Log.Information("Creating booking with UserId: {UserId}, TourId: {TourId}", booking.UserId, booking.TourId);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                booking.CreatedBy = currentUser.Id;
            }

            booking.CreatedDate = DateTime.UtcNow;
            booking.IsActive = true;
            await _unitOfWork.BookingRepository.AddAsync(booking);
            await _unitOfWork.CompleteAsync();

            Log.Information("Booking created successfully with BookingId: {BookingId}", booking.BookingId);

            return RedirectToAction(nameof(Index));
        }

        Log.Warning("Model state is invalid for creating booking");

        var users = await _userManager.Users.ToListAsync();
        ViewBag.UserId = new SelectList(users, "Id", "UserName", booking.UserId);
        ViewBag.TourId = new SelectList(await _unitOfWork.TourRepository.GetAllAsync(), "TourId", "Name", booking.TourId);
        return View(booking);
    }

    // GET: Bookings/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            Log.Warning("Edit action called with null id");
            return NotFound();
        }

        Log.Information("Editing booking with id: {Id}", id);

        var booking = await _unitOfWork.BookingRepository.GetByIdAsync(id.Value, "User,Tour");
        if (booking == null)
        {
            Log.Warning("Booking with id {Id} not found", id);
            return NotFound();
        }

        var users = await _userManager.Users.ToListAsync();
        ViewBag.UserId = new SelectList(users, "Id", "UserName", booking.UserId);
        ViewBag.TourId = new SelectList(await _unitOfWork.TourRepository.GetAllAsync(), "TourId", "Name", booking.TourId);
        return View(booking);
    }

    // POST: Bookings/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("BookingId,UserId,TourId,BookingDate,NumberOfParticipants,TotalPrice,Status,PaymentMethod,PaymentStatus,CreatedBy,CreatedDate,IsActive")] Booking booking)
    {
        if (id != booking.BookingId)
        {
            Log.Warning("Edit action called with mismatched ids: {Id} != {BookingId}", id, booking.BookingId);
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                Log.Information("Updating booking with BookingId: {BookingId}", booking.BookingId);

                _unitOfWork.BookingRepository.Update(booking);
                await _unitOfWork.CompleteAsync();

                Log.Information("Booking with BookingId: {BookingId} updated successfully", booking.BookingId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.BookingId))
                {
                    Log.Warning("Booking with BookingId: {BookingId} not found during update", booking.BookingId);
                    return NotFound();
                }
                else
                {
                    Log.Error("Concurrency exception occurred while updating booking with BookingId: {BookingId}", booking.BookingId);
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        Log.Warning("Model state is invalid for updating booking with BookingId: {BookingId}", booking.BookingId);

        var users = await _userManager.Users.ToListAsync();
        ViewBag.UserId = new SelectList(users, "Id", "UserName", booking.UserId);
        ViewBag.TourId = new SelectList(await _unitOfWork.TourRepository.GetAllAsync(), "TourId", "Name", booking.TourId);
        return View(booking);
    }

    // GET: Bookings/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            Log.Warning("Delete action called with null id");
            return NotFound();
        }

        Log.Information("Deleting booking with id: {Id}", id);

        var booking = await _unitOfWork.BookingRepository.GetByIdAsync(id.Value, "User,Tour");
        if (booking == null)
        {
            Log.Warning("Booking with id {Id} not found", id);
            return NotFound();
        }

        return View(booking);
    }

    // POST: Bookings/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        Log.Information("Deleting booking with BookingId: {BookingId}", id);

        var booking = await _unitOfWork.BookingRepository.GetByIdAsync(id);
        if (booking == null)
        {
            Log.Warning("Booking with BookingId: {BookingId} not found during delete", id);
            return NotFound();
        }

        _unitOfWork.BookingRepository.Remove(booking);
        await _unitOfWork.CompleteAsync();

        Log.Information("Booking with BookingId: {BookingId} deleted successfully", id);

        return RedirectToAction(nameof(Index));
    }

    private bool BookingExists(int id)
    {
        var exists = _unitOfWork.BookingRepository.GetByIdAsync(id) != null;
        Log.Information("Booking with id {Id} exists: {Exists}", id, exists);
        return exists;
    }
}