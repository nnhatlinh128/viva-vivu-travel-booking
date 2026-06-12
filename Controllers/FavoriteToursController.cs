using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories.IRepositories;

namespace ToursAndTravelsManagement.Controllers
{
    [Authorize]
    public class FavoriteToursController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoriteToursController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // ===============================
        // ADD / REMOVE FAVORITE (TOGGLE)
        // ===============================
        [HttpPost]
        public async Task<IActionResult> Toggle(int tourId)
        {
            // 1️⃣ LẤY USER HIỆN TẠI
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var userId = user.Id;

            // 2️⃣ KIỂM TRA ĐÃ FAVORITE CHƯA
            var favorites = await _unitOfWork.FavoriteTourRepository
                .GetAllAsync(f => f.UserId == userId && f.TourId == tourId);

            var existing = favorites.FirstOrDefault();

            // 3️⃣ TOGGLE
            if (existing != null)
            {
                _unitOfWork.FavoriteTourRepository.Remove(existing);
            }
            else
            {
                var favorite = new FavoriteTour
                {
                    UserId = userId,
                    TourId = tourId,
                };

                await _unitOfWork.FavoriteTourRepository.AddAsync(favorite);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Details", "Tours", new { id = tourId });
        }

        // ===============================
        // VIEW MY FAVORITES
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var favorites = await _unitOfWork.FavoriteTourRepository.GetAllAsync(
                f => f.UserId == userId,
                includeProperties: "Tour.Destination"
            );

            return View(favorites);
        }
    }
}
