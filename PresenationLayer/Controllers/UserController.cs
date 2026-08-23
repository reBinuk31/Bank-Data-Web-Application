using Microsoft.AspNetCore.Mvc;
namespace PresentationLayer.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public IActionResult Dashboard()
        {
            // Optionally verify session or cookie
            if (!Request.Cookies.ContainsKey("SessionID"))
                return RedirectToAction("GetDefaultView", "Login");

            ViewData["Title"] = "User Dashboard";
            return View("UserDashboard");
        }
    }
}
