using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DCAssignment2.Models;

namespace PresentationLayer.Controllers
{
    [Route("UserManagement")]
    public class UserManagementController : Controller
    {
        private const string businessApiBaseUrl = "http://localhost:5080/api/BusinessUser";

        // Default user management view: list all users
        [HttpGet("UserManagementDefaultView")]
        public IActionResult UserManagementDefaultView()
        {
            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var users = JsonConvert.DeserializeObject<List<User>>(response.Content);
            return View("UserManagementDefaultView", users);
        }

        // Search users by criteria
        [HttpGet("SearchUser")]
        public IActionResult SearchUser(string searchUsername, string searchEmail, string searchPhone, string searchAccount, string searchAddress)
        {
            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var users = JsonConvert.DeserializeObject<List<User>>(response.Content);

            var filtered = users.Where(u =>
                (string.IsNullOrEmpty(searchUsername) || u.Username.Contains(searchUsername, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(searchEmail) || u.Email.Contains(searchEmail, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(searchPhone) || u.PhoneNo.Contains(searchPhone, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(searchAddress) || u.Address.Contains(searchAddress, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(searchAccount) || u.Accounts.Any(a => a.AccountNumber.Contains(searchAccount)))
            ).ToList();

            return View("UserManagementDefaultView", filtered);
        }

        // Render create user form
        [HttpGet("CreateUser")]
        public IActionResult CreateUser()
        {
            return View("CreateUserView", new User());
        }

        // Handle create user form submission
        [HttpPost("CreateUser")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(User newUser)
        {
            if (!ModelState.IsValid)
                return View("CreateUserView", newUser);

            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest("", Method.Post);
            request.AddJsonBody(newUser);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                ViewBag.ErrorMessage = "Failed to create user.";
                return View("CreateUserView", newUser);
            }

            return RedirectToAction("UserManagementDefaultView");
        }

        // Render edit user form
        [HttpGet("EditUser/{id}")]
        public IActionResult EditUser(int id)
        {
            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest($"{id}", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var user = JsonConvert.DeserializeObject<User>(response.Content);
            return View("EditUserView", user);
        }

        // Handle edit user submission
        [HttpPost("EditUser")]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User updatedUser)
        {
            if (!ModelState.IsValid)
                return View("EditUserView", updatedUser);

            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest($"{updatedUser.Id}", Method.Put);
            request.AddJsonBody(updatedUser);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                ViewBag.ErrorMessage = "Failed to update user.";
                return View("EditUserView", updatedUser);
            }

            return RedirectToAction("UserManagementDefaultView");
        }

        // Render reset password form
        [HttpGet("ResetPassword/{id}")]
        public IActionResult ResetPassword(int id)
        {
            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest($"{id}", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var user = JsonConvert.DeserializeObject<User>(response.Content);
            return View("ResetPasswordView", user);
        }

        // Handle reset password submission
        [HttpPost("ResetPassword")]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(int Id, string NewPassword)
        {
            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest($"{Id}", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var user = JsonConvert.DeserializeObject<User>(response.Content);
            user.Password = NewPassword;

            var updateRequest = new RestRequest($"{Id}", Method.Put);
            updateRequest.AddJsonBody(user);
            var updateResponse = client.Execute(updateRequest);

            if (!updateResponse.IsSuccessful)
            {
                ViewBag.ErrorMessage = "Failed to reset password.";
                return View("ResetPasswordView", user);
            }

            return RedirectToAction("UserManagementDefaultView");
        }

        // Deactivate (delete) user
        [HttpPost("DeleteUser/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest($"{id}", Method.Delete);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                TempData["ErrorMessage"] = "Failed to deactivate user.";
            }

            return RedirectToAction("UserManagementDefaultView");
        }
    }
}
