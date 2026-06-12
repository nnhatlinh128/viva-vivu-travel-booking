using Microsoft.AspNetCore.Mvc;

namespace ToursAndTravelsManagement.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
