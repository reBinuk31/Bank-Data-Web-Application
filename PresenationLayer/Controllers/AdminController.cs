using Microsoft.AspNetCore.Mvc;
using DCAssignment2.Models;

namespace PresentationLayer.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet("Admin/Dashboard")]
        public IActionResult Dashboard()
        {
            if (!Request.Cookies.ContainsKey("SessionID"))
                return RedirectToAction("GetDefaultView", "Login");

            ViewData["Title"] = "Admin Dashboard";
            return View("AdminDashboard");
        }
    }
}
