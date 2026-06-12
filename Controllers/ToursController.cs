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
public class ToursController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfService _pdfService;
    private readonly IExcelExportService _excelExportService;
    public ToursController(IUnitOfWork unitOfWork, IPdfService pdfService, IExcelExportService excelExportService)
    {
        _unitOfWork = unitOfWork;
        _pdfService = pdfService;
        _excelExportService = excelExportService;
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf()
    {
        var userName = User?.Identity?.Name ?? "Unknown User"; // Get the current logged-in user

        Log.Information("User {UserName} is generating a PDF for all tours", userName);

        // Fetch all tours from the database, including the related Destination
        var tours = await _unitOfWork.TourRepository.GetAllAsync(null, includeProperties: "Destination");

        if (tours == null || !tours.Any())
        {
            Log.Warning("User {UserName} tried to generate a PDF, but there are no tours available", userName);
            return NotFound("No tours found to export.");
        }

        // Create a string with the list of tours and their details
        string content = "Tours List:\n\n";
        foreach (var tour in tours)
        {
            content += $"Name: {tour.Name}\n" +
                       $"Description: {tour.Description}\n" +
                       $"Destination: {tour.Destination?.Name}\n" +
                       $"Start Date: {tour.StartDate.ToString("dd MMM yyyy")}\n" +
                       $"End Date: {tour.EndDate.ToString("dd MMM yyyy")}\n" +
                       $"Price: {tour.Price:C}\n" +
                       $"Max Participants: {tour.MaxParticipants}\n\n";
        }

        // Generate the PDF using the PDF service
        var pdf = _pdfService.GenerateToursPdf("Tours Report", content);

        // Return the PDF as a downloadable file
        return File(pdf, "application/pdf", "ToursReport.pdf");
    }


    // Export Excel Action
    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        var tours = await _unitOfWork.TourRepository.GetAllAsync(includeProperties: "Destination");
        var excelContent = _excelExportService.ExportToursToExcel(tours.ToList());
        return File(excelContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Tours.xlsx");
    }

    // GET: Tours
    public async Task<IActionResult> Index(int? pageNumber, int? pageSize)
    {
        var userName = User?.Identity?.Name ?? "Unknown User"; // Get current logged-in user

        Log.Information("User {UserName} accessed the Tours Index page", userName);

        int pageIndex = pageNumber ?? 1;
        int size = pageSize ?? 10;

        var tours = await _unitOfWork.TourRepository.GetPaginatedAsync(pageIndex, size, "Destination");
        return View(tours);
    }

// ================= CREATE TOUR BY TYPE =================

// Tạo tour nội địa
[Authorize(Policy = "RequireAdminRole")]
public async Task<IActionResult> CreateDomestic()
{
    var destinations = await _unitOfWork.DestinationRepository
        .GetAllAsync(d => d.IsDomestic && d.IsActive);

    ViewBag.DestinationId = new SelectList(destinations, "DestinationId", "Name");

    return View("Create");
}

// Tạo tour quốc tế
[Authorize(Policy = "RequireAdminRole")]
public async Task<IActionResult> CreateInternational()
{
    var destinations = await _unitOfWork.DestinationRepository
        .GetAllAsync(d => !d.IsDomestic && d.IsActive);

    ViewBag.DestinationId = new SelectList(destinations, "DestinationId", "Name");

    return View("Create");
}

    // GET: Tours/Create
    public async Task<IActionResult> Create()
    {
        var userName = User?.Identity?.Name ?? "Unknown User";

        var destinations = await _unitOfWork.DestinationRepository.GetAllAsync();
        ViewBag.DestinationId = new SelectList(destinations, "DestinationId", "Name");

        Log.Information("User {UserName} is accessing the Tour Creation page", userName);
        return View();
    }

    // POST: Tours/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,Price,MaxParticipants,DestinationId")] Tour tour)
    {
        var userName = User?.Identity?.Name ?? "Unknown User";

        if (ModelState.IsValid)
        {
            tour.CreatedBy = userName;
            tour.CreatedDate = DateTime.UtcNow;
            tour.IsActive = true;

            await _unitOfWork.TourRepository.AddAsync(tour);
            await _unitOfWork.CompleteAsync();

            Log.Information("User {UserName} created a new tour: {@Tour}", userName, tour);
            return RedirectToAction(nameof(Index));
        }

        var destinations = await _unitOfWork.DestinationRepository.GetAllAsync();
        ViewBag.DestinationId = new SelectList(destinations, "DestinationId", "Name", tour.DestinationId);

        Log.Warning("User {UserName} attempted to create a tour with invalid data: {@Tour}", userName, tour);
        return View(tour);
    }

    // GET: Tours/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        var userName = User?.Identity?.Name ?? "Unknown User";

        if (id == null)
        {
            Log.Warning("User {UserName} tried to access Tour Edit with null ID", userName);
            return NotFound();
        }

        var tour = await _unitOfWork.TourRepository.GetByIdAsync(id.Value);
        if (tour == null)
        {
            Log.Warning("User {UserName} tried to access Tour Edit with invalid ID {TourId}", userName, id);
            return NotFound();
        }

        var destinations = await _unitOfWork.DestinationRepository.GetAllAsync();
        ViewBag.DestinationId = new SelectList(destinations, "DestinationId", "Name", tour.DestinationId);

        Log.Information("User {UserName} is editing Tour {TourId}", userName, id);
        return View(tour);
    }

    // POST: Tours/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("TourId,Name,Description,StartDate,EndDate,Price,MaxParticipants,DestinationId,CreatedBy,CreatedDate,IsActive")] Tour tour)
    {
        var userName = User?.Identity?.Name ?? "Unknown User";

        if (id != tour.TourId)
        {
            Log.Warning("User {UserName} tried to edit a tour with mismatched ID {TourId}", userName, id);
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _unitOfWork.TourRepository.Update(tour);
                await _unitOfWork.CompleteAsync();

                Log.Information("User {UserName} successfully edited Tour {TourId}", userName, id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TourExists(tour.TourId))
                {
                    Log.Error("User {UserName} attempted to edit a non-existent tour {TourId}", userName, id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        var destinations = await _unitOfWork.DestinationRepository.GetAllAsync();
        ViewBag.DestinationId = new SelectList(destinations, "DestinationId", "Name", tour.DestinationId);

        Log.Warning("User {UserName} submitted invalid data for editing Tour {TourId}", userName, id);
        return View(tour);
    }

    // GET: Tours/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        var userName = User?.Identity?.Name ?? "Unknown User";

        if (id == null)
        {
            Log.Warning("User {UserName} tried to access Tour Delete with null ID", userName);
            return NotFound();
        }

        var tour = await _unitOfWork.TourRepository.GetByIdAsync(id.Value);
        if (tour == null)
        {
            Log.Warning("User {UserName} tried to access Tour Delete with invalid ID {TourId}", userName, id);
            return NotFound();
        }

        Log.Information("User {UserName} is deleting Tour {TourId}", userName, id);
        return View(tour);
    }

    // POST: Tours/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userName = User?.Identity?.Name ?? "Unknown User";

        var tour = await _unitOfWork.TourRepository.GetByIdAsync(id);
        if (tour == null)
        {
            Log.Warning("User {UserName} tried to delete a non-existent tour {TourId}", userName, id);
            return NotFound();
        }

        _unitOfWork.TourRepository.Remove(tour);
        await _unitOfWork.CompleteAsync();

        Log.Information("User {UserName} successfully deleted Tour {TourId}", userName, id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> TourExists(int id)
    {
        return await _unitOfWork.TourRepository.GetByIdAsync(id) != null;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Domestic(
        string? destination,
        int? month,
        string? price
    )
    {
        ViewData["Title"] = "Tour Nội Địa";

        var tours = await _unitOfWork.TourRepository.GetAllAsync(
            t => t.IsActive && t.Destination.IsDomestic,
            includeProperties: "Destination"
        );

        // 🔹 Filter theo điểm đến
        if (!string.IsNullOrEmpty(destination))
        {
            tours = tours
                .Where(t => t.Destination.Name.Contains(destination))
                .ToList();
        }

        // 🔹 Filter theo tháng
        if (month.HasValue)
        {
            tours = tours
                .Where(t => t.StartDate.Month == month.Value)
                .ToList();
        }

        // 🔹 Filter theo giá
        if (!string.IsNullOrEmpty(price))
        {
            tours = price switch
            {
                "under10" => tours.Where(t => t.Price < 10_000_000).ToList(),
                "10to20" => tours.Where(t => t.Price >= 10_000_000 && t.Price <= 20_000_000).ToList(),
                "20to30" => tours.Where(t => t.Price > 20_000_000 && t.Price <= 30_000_000).ToList(),
                "over30" => tours.Where(t => t.Price > 30_000_000).ToList(),
                _ => tours
            };
        }

        return View(tours);
    }

    [AllowAnonymous]
    public async Task<IActionResult> International(
        string? destination,
        int? month,
        string? price
    )
    {
        ViewData["Title"] = "Tour Quốc Tế";

        var tours = await _unitOfWork.TourRepository.GetAllAsync(
            t => t.IsActive && !t.Destination.IsDomestic,
            includeProperties: "Destination"
        );

        if (!string.IsNullOrEmpty(destination))
        {
            tours = tours
                .Where(t => t.Destination.Name.Contains(destination))
                .ToList();
        }

        if (month.HasValue)
        {
            tours = tours
                .Where(t => t.StartDate.Month == month.Value)
                .ToList();
        }

        if (!string.IsNullOrEmpty(price))
        {
            tours = price switch
            {
                "under10" => tours.Where(t => t.Price < 10_000_000).ToList(),
                "10to20" => tours.Where(t => t.Price >= 10_000_000 && t.Price <= 20_000_000).ToList(),
                "20to30" => tours.Where(t => t.Price > 20_000_000 && t.Price <= 30_000_000).ToList(),
                "over30" => tours.Where(t => t.Price > 30_000_000).ToList(),
                _ => tours
            };
        }

        return View(tours);
    }

[AllowAnonymous]
// GET: Tours/Details/5
public async Task<IActionResult> Details(int? id)
{
    var userName = User?.Identity?.Name ?? "Unknown User";

    if (id == null)
    {
        Log.Warning("User {UserName} tried to access Tour Details with null ID", userName);
        return NotFound();
    }

    // 1️⃣ Lấy tour + destination
    var tour = await _unitOfWork.TourRepository
        .GetByIdAsync(id.Value, "Destination");

    if (tour == null)
    {
        Log.Warning("User {UserName} tried to access Tour Details with invalid ID {TourId}", userName, id);
        return NotFound();
    }

    // 2️⃣ LẤY LỊCH TRÌNH TOUR
    var itineraries = await _unitOfWork.TourItineraryRepository
        .GetAllAsync(i => i.TourId == id.Value);

    // 3️⃣ GỬI SANG VIEW
    ViewBag.Itineraries = itineraries
        .OrderBy(i => i.DayNumber)
        .ToList();

    Log.Information("User {UserName} accessed details of Tour {TourId}", userName, id);

    return View(tour);
}

    [AllowAnonymous]
[HttpGet]
public async Task<IActionResult> PublicTours()
{
    var tours = await _unitOfWork.TourRepository.GetAllAsync(
        t => t.IsActive == true,
        includeProperties: "Destination"
    );

    return View(tours);
}

}
