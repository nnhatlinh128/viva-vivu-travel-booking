using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Services.EmailService;
using ToursAndTravelsManagement.Repositories.IRepositories;

namespace ToursAndTravelsManagement.Controllers;

public class HomeController : Controller
{
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork; // Lấy dữ liệu tour

    public HomeController(IEmailService emailService, IUnitOfWork unitOfWork)
    {
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    // GET: Home/Index
    public async Task<IActionResult> Index() // Đổi thành async Task<IActionResult>
    {
        Log.Information("Home page (Index) accessed by user {UserId} at {Timestamp}", User.Identity?.Name, DateTime.UtcNow);

        // Lấy tour từ database 
        var allTours = await _unitOfWork.TourRepository.GetAllAsync(
            filter: null,            
            includeProperties: "Destination" // Include để hiển thị tên điểm đến
        );

        // Truyền tour vào ViewBag để slider sử dụng
        ViewBag.BestOfferTours = allTours ?? new List<Tour>();

        return View();
    }

    // GET: Home/Privacy
    public IActionResult Privacy()
    {
        Log.Information("Privacy page accessed by user {UserId} at {Timestamp}", User.Identity?.Name, DateTime.UtcNow);
        return View();
    }

    // GET: Home/Error
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        Log.Error("Error page accessed. RequestId: {RequestId}, User: {UserId}, Timestamp: {Timestamp}", requestId, User.Identity?.Name, DateTime.UtcNow);

        return View(new ErrorViewModel { RequestId = requestId });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitForm(ContactFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model); // Return to form view with model state
        }

        // Load the email template
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "emailTemplates", "emailTemplate.html");
        var emailTemplate = await System.IO.File.ReadAllTextAsync(templatePath);

        // Replace placeholders with actual values
        emailTemplate = emailTemplate.Replace("{{FirstName}}", model.FirstName)
                                     .Replace("{{LastName}}", model.LastName)
                                     .Replace("{{Email}}", model.Email)
                                     .Replace("{{Subject}}", model.Subject)
                                     .Replace("{{Message}}", model.Message);

        // Send email
        await _emailService.SendEmailAsync(model.Email, model.Subject, emailTemplate);

        return RedirectToAction("ThankYou");
    }

    public IActionResult Chitiet()
    {
        return View();
    }

    public IActionResult ComingSoon()
    {
        return View();
    }
}