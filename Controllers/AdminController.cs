using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.ViewModels;
using ToursAndTravelsManagement.Enums;
using ToursAndTravelsManagement.Repositories.IRepositories;

namespace ToursAndTravelsManagement.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IUnitOfWork _unitOfWork;


    public AdminController(
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _env = env;
        _unitOfWork = unitOfWork;
    }

        // ================= USER MANAGEMENT =================
        
        [HttpGet]
        public async Task<IActionResult> Users(string roleFilter)
        {
            var users = _userManager.Users.ToList();
            var model = new List<AdminUserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User";

                model.Add(new AdminUserViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Role = role,
                    IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now
                });
            }

            // Lọc theo role (nếu có)
            if (!string.IsNullOrEmpty(roleFilter))
            {
                model = model
                    .Where(u => u.Role == roleFilter)
                    .ToList();
            }

            ViewBag.RoleFilter = roleFilter;

            return View("~/Views/Admin/AdminUsers.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Users");

            // Không cho admin tự hạ chính mình
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id == user.Id && newRole != "Admin")
            {
                TempData["Error"] = "Bạn không thể tự thay đổi quyền của chính mình.";
                return RedirectToAction("Users");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.LockoutEnd = DateTimeOffset.MaxValue;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.LockoutEnd = null;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Users");
        }

        // ================= PROFILE =================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new RegisterViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                City = user.City,
                Country = user.Country
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(RegisterViewModel model)
        {
            // Bỏ validation của Register
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Update thông tin
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.DateOfBirth = model.DateOfBirth;
            user.Address = model.Address;
            user.City = model.City;
            user.Country = model.Country;

            // ===== AVATAR =====
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.AvatarFile.FileName);
                var filePath = Path.Combine(uploadFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.AvatarFile.CopyToAsync(stream);

                user.AvatarUrl = "/uploads/avatars/" + fileName;
            }

            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Cập nhật thông tin thành công";
            return RedirectToAction("Profile");
        }

        // ================= DASHBOARD =================

        public async Task<IActionResult> Dashboard(DateTime? fromDate, DateTime? toDate)
        {
            var bookings = await _unitOfWork.BookingRepository.GetAllAsync();

            var startDate = fromDate ?? DateTime.Today.AddDays(-30);
            var endDate = toDate ?? DateTime.Today;

            var filteredBookings = bookings
                .Where(b => b.BookingDate.Date >= startDate.Date
                        && b.BookingDate.Date <= endDate.Date)
                .ToList();

            var paidBookings = filteredBookings
                .Where(b => b.PaymentStatus == PaymentStatus.Completed)
                .ToList();

            // ==== DATA CHO BIỂU ĐỒ ====
            var revenueByDate = paidBookings
                .GroupBy(b => b.BookingDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key.ToString("dd/MM"),
                    Revenue = g.Sum(x => x.TotalPrice)
                })
                .ToList();

            var model = new DashboardViewModel
            {
                TotalBookings = filteredBookings.Count,
                CancelledBookings = filteredBookings.Count(b => b.Status == BookingStatus.Cancelled),
                TotalRevenue = paidBookings.Sum(b => b.TotalPrice),
                Profit = paidBookings.Sum(b => b.TotalPrice) * 0.2m,

                RevenueLabels = revenueByDate.Select(x => x.Date).ToList(),
                RevenueValues = revenueByDate.Select(x => x.Revenue).ToList()
            };

            ViewBag.FromDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.ToDate = endDate.ToString("yyyy-MM-dd");

            return View(model);
        }
        // ================= DASHBOARD SUB PAGES =================

        [HttpGet]
        public IActionResult Analyze()
        {
            // Trang phân tích chi tiết (sẽ mở rộng sau)
            return View();
        }

        [HttpGet]
        public IActionResult Export()
        {
            // Trang export báo cáo (PDF / Excel)
            return View();
        }

        [HttpGet]
        public IActionResult Setting()
        {
            // Trang cài đặt dashboard / hệ thống
            return View();
        }
    }
}
