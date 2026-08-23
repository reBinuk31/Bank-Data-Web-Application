using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DCAssignment2.Models;

namespace PresentationLayer.Controllers
{
    public class AccountSummaryController : Controller
    {
        private const string businessApiBaseUrl = "http://localhost:5080/api/BusinessAccount";
        private const string businessUserApi = "http://localhost:5080/api/BusinessUser";

        // helper to check which user is logged in
        private string? GetLoggedInUsername()
        {
            return Request.Cookies["SessionID"];
        }

        // GET: /AccountSummary/AccountView
        [HttpGet]
        public IActionResult AccountView()
        {
            var username = GetLoggedInUsername();
            if (string.IsNullOrEmpty(username))
            {
                // Not logged in → redirect to Login page
                return RedirectToAction("GetDefaultView", "Login");
            }

            // Retrieve all accounts from Business Web API
            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                return StatusCode((int)response.StatusCode, response.Content);
            }

            var allAccounts = JsonConvert.DeserializeObject<List<Account>>(response.Content);

            // Filter accounts by the current logged-in user's username
            var userAccounts = allAccounts?
                .Where(a => a.User != null && a.User.Username == username)
                .ToList();

            if (userAccounts == null || userAccounts.Count == 0)
            {
                ViewBag.Message = "No accounts found for this user.";
                return View("AccountView", new List<Account>());
            }

            // Keep UserRole alive for navbar display
            TempData["UserRole"] = userAccounts.First().User.isAdmin ? "Admin" : "User";

            return View("AccountView", userAccounts);
        }

        // NEW: Redirect to TransactionHistory with selected account
        [HttpGet("ViewAccountTransactions/{accountId}")]
        public IActionResult ViewAccountTransactions(int accountId)
        {
            var username = GetLoggedInUsername();
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("GetDefaultView", "Login");
            }

            // Verify the account belongs to the current user
            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                TempData["ErrorMessage"] = "Error loading account information.";
                return RedirectToAction("AccountView");
            }

            var allAccounts = JsonConvert.DeserializeObject<List<Account>>(response.Content);
            var userAccount = allAccounts?.FirstOrDefault(a => a.Id == accountId && a.User?.Username == username);

            if (userAccount == null)
            {
                TempData["ErrorMessage"] = "Account not found or access denied.";
                return RedirectToAction("AccountView");
            }

            // Store the selected account ID in TempData for TransactionHistory to use
            TempData["SelectedAccountId"] = accountId;
            TempData["SuccessMessage"] = $"Now viewing transactions for account: {userAccount.AccountNumber}";

            return RedirectToAction("TransactionDefaultView", "TransactionHistory");
        }
    }
}