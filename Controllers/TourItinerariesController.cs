using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories.IRepositories;

namespace ToursAndTravelsManagement.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    public class TourItinerariesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public TourItinerariesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ===================== INDEX =====================
        // /TourItineraries/Index?tourId=1
        public async Task<IActionResult> Index(int tourId)
        {
            var itineraries = await _unitOfWork.TourItineraryRepository
                .GetAllAsync(i => i.TourId == tourId, includeProperties: "Tour");

            ViewBag.TourId = tourId;
            ViewBag.TourName = itineraries.FirstOrDefault()?.Tour?.Name;

            return View(itineraries.OrderBy(i => i.DayNumber));
        }

        // ===================== CREATE =====================
        public IActionResult Create(int tourId)
        {
            ViewBag.TourId = tourId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
public async Task<IActionResult> Create(TourItinerary itinerary)
{
    if (!ModelState.IsValid)
    {
        ViewBag.TourId = itinerary.TourId;
        return View(itinerary);
    }

    try
    {
        await _unitOfWork.TourItineraryRepository.AddAsync(itinerary);
        await _unitOfWork.CompleteAsync();
    }
    catch (Exception ex)
    {
        // 🔥 IN RA LỖI THẬT CỦA DATABASE
        var error = ex.InnerException?.Message ?? ex.Message;
        return Content(error);
    }

    return RedirectToAction(nameof(Index), new { tourId = itinerary.TourId });
}


        // ===================== EDIT =====================
        public async Task<IActionResult> Edit(int id)
        {
            var itinerary = await _unitOfWork.TourItineraryRepository.GetByIdAsync(id);
            if (itinerary == null)
                return NotFound();

            return View(itinerary);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TourItinerary itinerary)
        {
            if (id != itinerary.TourItineraryId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(itinerary);

            _unitOfWork.TourItineraryRepository.Update(itinerary);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index), new { tourId = itinerary.TourId });
        }

        // ===================== DELETE =====================
        public async Task<IActionResult> Delete(int id)
        {
            var itinerary = await _unitOfWork.TourItineraryRepository
                .GetByIdAsync(id, includeProperties: "Tour");

            if (itinerary == null)
                return NotFound();

            return View(itinerary);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var itinerary = await _unitOfWork.TourItineraryRepository.GetByIdAsync(id);
            if (itinerary == null)
                return NotFound();

            int tourId = itinerary.TourId;

            _unitOfWork.TourItineraryRepository.Remove(itinerary);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index), new { tourId });
        }
    }
}
