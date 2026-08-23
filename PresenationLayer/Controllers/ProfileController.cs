using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DCAssignment2.Models;

namespace PresentationLayer.Controllers
{
    public class ProfileController : Controller
    {
        private const string businessApiBaseUrl = "http://localhost:5080/api/BusinessUser";

        // Helper to get the logged-in username from cookie
        private string? GetLoggedInUsername()
        {
            return Request.Cookies["SessionID"];
        }

        // GET: /Profile/ProfileView
        [HttpGet]
        public IActionResult ProfileView()
        {
            var username = GetLoggedInUsername();
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("GetDefaultView", "Login");
            }

            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                return StatusCode((int)response.StatusCode, "Error retrieving users from Business API");
            }

            var users = JsonConvert.DeserializeObject<List<User>>(response.Content);
            var currentUser = users?.FirstOrDefault(u => u.Username == username);

            if (currentUser == null)
                return RedirectToAction("GetDefaultView", "Login");

            TempData["UserRole"] = currentUser.isAdmin ? "Admin" : "User";
            return View("ProfileView", currentUser);
        }

        // GET: /Profile/Edit
        [HttpGet]
        public IActionResult Edit()
        {
            var username = GetLoggedInUsername();
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("GetDefaultView", "Login");

            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var users = JsonConvert.DeserializeObject<List<User>>(response.Content);
            var currentUser = users?.FirstOrDefault(u => u.Username == username);

            if (currentUser == null)
                return RedirectToAction("GetDefaultView", "Login");

            return View("Edit", currentUser);
        }

        // POST: /Profile/SaveEdit
        [HttpPost]
        public IActionResult SaveEdit(User model, string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (model == null)
                return BadRequest("Invalid user data.");

            var username = GetLoggedInUsername();
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("GetDefaultView", "Login");

            // Fetch user list from Business API
            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, "Error contacting Business API.");

            var users = JsonConvert.DeserializeObject<List<User>>(response.Content);
            var user = users?.FirstOrDefault(u => u.Username == username);

            if (user == null)
                return RedirectToAction("GetDefaultView", "Login");

            // ✅ Validate password change only if user entered something
            bool isPasswordChangeAttempt =
                !string.IsNullOrWhiteSpace(CurrentPassword) ||
                !string.IsNullOrWhiteSpace(NewPassword) ||
                !string.IsNullOrWhiteSpace(ConfirmPassword);

            if (isPasswordChangeAttempt)
            {
                // Check current password
                if (user.Password != CurrentPassword)
                {
                    ViewBag.Error = "Current password is incorrect. Please try again";
                    return View("Edit", model);
                }

                // Check new password match
                if (NewPassword != ConfirmPassword)
                {
                    ViewBag.Error = "New passwords do not match.Please try again";
                    return View("Edit", model);
                }

                // Only now update the password
                user.Password = NewPassword;
            }

            // ✅ Update non-sensitive info safely
            user.Email = model.Email;
            user.Address = model.Address;
            user.PhoneNo = model.PhoneNo;

            // Prevent validation errors by removing circular references
            if (user.Accounts != null)
            {
                foreach (var acc in user.Accounts)
                {
                    acc.User = null;
                }
            }


            // Prepare object for PUT request (exclude Accounts to avoid validation errors)
            var userUpdate = new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Address = user.Address,
                PhoneNo = user.PhoneNo,
                Password = user.Password,
                Icon = user.Icon,
                isAdmin = user.isAdmin
                // ⚠️ Do NOT include Accounts
            };

            // PUT request
            // PUT request
            var updateRequest = new RestRequest($"{user.Id}", Method.Put);
            updateRequest.AddJsonBody(userUpdate);
            var updateResponse = client.Execute(updateRequest);

            if (!updateResponse.IsSuccessful)
            {
                ViewBag.Error = $"⚠️ Failed to update profile. Details: {updateResponse.Content}";
                return View("Edit", model);
            }

            // ✅ Successful update
            TempData["UserRole"] = user.isAdmin ? "Admin" : "User";
            return RedirectToAction("ProfileView");


        }

    }
}
