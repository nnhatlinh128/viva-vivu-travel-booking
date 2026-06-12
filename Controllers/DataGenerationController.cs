using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ToursAndTravelsManagement.Data;

namespace ToursAndTravelsManagement.Controllers;

[Authorize(Policy = "RequireAdminRole")]
public class DataGenerationController : Controller
{
    private readonly DataSeeder _dataSeeder;

    public DataGenerationController(DataSeeder dataSeeder)
    {
        _dataSeeder = dataSeeder;
    }

    // GET: DataGeneration/GenerateDestinations
    public IActionResult GenerateDestinations()
    {
        return View();
    }

    // POST: DataGeneration/GenerateDestinations
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateDestinations(int numberOfDestinations)
    {
        await _dataSeeder.SeedDestinationsAsync(numberOfDestinations);
        return RedirectToAction("Index", "Destinations");
    }

    // GET: DataGeneration/GenerateTours
    public IActionResult GenerateTours()
    {
        return View();
    }

    // POST: DataGeneration/GenerateTours
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateTours(int numberOfTours)
    {
        await _dataSeeder.SeedToursAsync(numberOfTours);
        return RedirectToAction("Index", "Tours");
    }

    // GET: DataGeneration/GenerateBookings
    public IActionResult GenerateBookings()
    {
        return View();
    }

    // POST: DataGeneration/GenerateBookings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateBookings(int numberOfBookings)
    {
        await _dataSeeder.SeedBookingsAsync(numberOfBookings);
        return RedirectToAction("Index", "Bookings");
    }
}
