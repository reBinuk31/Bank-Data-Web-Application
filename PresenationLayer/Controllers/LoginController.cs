using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DCAssignment2.Models;

namespace PresentationLayer.Controllers
{
    public class LoginController : Controller
    {
        private const string businessApiBaseUrl = "http://localhost:5080/api/BusinessUser";

        [HttpGet("defaultview")]
        public IActionResult GetDefaultView()
        {
            // If session already existsS
            if (Request.Cookies.ContainsKey("SessionID"))
            {
                return PartialView("LoginViewAuthenticated");
            }
            return PartialView("LoginDefaultView");
        }

        [HttpGet("error")]
        public IActionResult GetErrorView()
        {
            return PartialView("LoginErrorView");
        }

        [HttpPost("auth")]
        public IActionResult Authenticate(User loginUser)
        {
            if (loginUser == null)
                return BadRequest("Invalid user data.");

            // Get all users from BusinessWebAPI
            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, "Error contacting business API.");

            var users = JsonConvert.DeserializeObject<List<User>>(response.Content);

            var foundUser = users?.FirstOrDefault(u =>
                u.Username == loginUser.Username && u.Password == loginUser.Password);

            if (foundUser != null)
            {
                // Save role in cookie/session
                Response.Cookies.Append("SessionID", foundUser.Username);
                TempData["UserRole"] = foundUser.isAdmin ? "Admin" : "User";

                // Determine dashboard URL
                string redirectUrl = foundUser.isAdmin ? Url.Action("Dashboard", "Admin")
                                                      : Url.Action("Dashboard", "User");

                // Return JSON for front-end JS to handle redirect
                return Redirect(redirectUrl);
            }

            return Json(new { login = false });
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            // Clear the session/cookie
            Response.Cookies.Delete("SessionID");
            TempData["UserRole"] = null;

            // Redirect to the Home/Index page (login page)
            return RedirectToAction("Index", "Home");
        }


    }
}
