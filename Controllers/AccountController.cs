using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.ViewModels;

namespace ToursAndTravelsManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
             _env = env;
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            Log.Information("Register page accessed.");
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    Address = model.Address,
                    City = model.City,
                    Country = model.Country,
                    RegistrationDate = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    Log.Information("User {Email} registered successfully.", user.Email);

                // Gán role mặc định là User
                    await _userManager.AddToRoleAsync(user, "User");
                    Log.Information("User {Email} assigned to role {Role}.", user.Email);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    Log.Information("User {Email} signed in after registration.", user.Email);

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    Log.Error("Registration error for {Email}: {Error}", user.Email, error.Description);
                }
            }

            return View(model);
        }

        // GET: Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            Log.Information("Login page accessed.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    Log.Information("User {Email} logged in successfully.", model.Email);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    Log.Warning("Invalid login attempt for {Email}.", model.Email);
                    return View(model);
                }
            }

            Log.Warning("Login failed due to invalid model state.");
            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            Log.Information("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                Log.Information("Redirecting to local URL: {ReturnUrl}.", returnUrl);
                return Redirect(returnUrl);
            }
            else
            {
                Log.Information("Redirecting to home page.");
                return RedirectToAction("Index", "Home");
            }
        }
        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateUserProfileViewModel model)

        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;
            user.Address = model.Address;

            await _userManager.UpdateAsync(user);

            return RedirectToAction("MyBookings", "UserBookings");
        }

        // POST: Account/UpdateAvatarUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAvatar(IFormFile AvatarFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(AvatarFile.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await AvatarFile.CopyToAsync(stream);

                user.AvatarUrl = "/uploads/avatars/" + fileName;
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("MyBookings", "UserBookings");
        }
    }
}
