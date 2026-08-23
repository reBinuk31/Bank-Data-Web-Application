using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DCAssignment2.Models;

namespace PresentationLayer.Controllers
{
    [Route("TransactionHistory")]
    public class TransactionHistoryController : Controller
    {
        private const string businessApiBaseUrl = "http://localhost:5080/api/BusinessTransaction";
        private const string userApiBaseUrl = "http://localhost:5080/api/BusinessUser";

        // Helper to get logged-in username
        private string? GetLoggedInUsername() => Request.Cookies["SessionID"];

        // GET: /TransactionHistory/TransactionDefaultView
        [HttpGet]
        public IActionResult TransactionDefaultView(int? accountId)
        {
            try
            {
                var username = GetLoggedInUsername();
                if (string.IsNullOrEmpty(username))
                    return RedirectToAction("GetDefaultView", "Login");

                // Get current user
                var userClient = new RestClient(userApiBaseUrl);
                var userRequest = new RestRequest("", Method.Get);
                var userResponse = userClient.Execute(userRequest);

                if (!userResponse.IsSuccessful)
                {
                    ViewBag.ErrorMessage = "Error contacting Business API for user data.";
                    return View("TransactionErrorView", new User());
                }

                var users = JsonConvert.DeserializeObject<List<User>>(userResponse.Content);
                var currentUser = users?.FirstOrDefault(u => u.Username == username);

                if (currentUser == null)
                    return RedirectToAction("GetDefaultView", "Login");

                if (accountId.HasValue)
                {
                    ViewBag.SelectedAccountId = accountId.Value;
                }
                else
                {
                    ViewBag.SelectedAccountId = currentUser.Accounts.FirstOrDefault()?.Id ?? 0;
                }


                // ✅ FIX: Get transactions with circular reference handling
                List<Transaction> allTransactions = new List<Transaction>();

                try
                {
                    var transactionClient = new RestClient(businessApiBaseUrl);
                    var transactionRequest = new RestRequest("", Method.Get);
                    var transactionResponse = transactionClient.Execute(transactionRequest);

                    if (transactionResponse.IsSuccessful && !string.IsNullOrEmpty(transactionResponse.Content))
                    {
                        // Configure JSON settings to handle circular references
                        var settings = new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            MaxDepth = 2
                        };

                        allTransactions = JsonConvert.DeserializeObject<List<Transaction>>(
                            transactionResponse.Content, settings) ?? new List<Transaction>();
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Error retrieving transactions from Business API.";
                    }
                }
                catch (JsonException jsonEx)
                {
                    // If JSON deserialization fails, create empty list
                    allTransactions = new List<Transaction>();
                    ViewBag.ErrorMessage = "Error processing transaction data.";
                }

                // Filter transactions for current user's accounts
                var userAccountIds = currentUser.Accounts?.Select(a => a.Id).ToList() ?? new List<int>();
                var userTransactions = allTransactions
                    .Where(t => userAccountIds.Contains(t.FromAccountId) ||
                               userAccountIds.Contains(t.ToAccountId))
                    .OrderByDescending(t => t.Timestamp)
                    .ToList();

                ViewBag.UserTransactions = userTransactions;
                TempData["UserRole"] = currentUser.isAdmin ? "Admin" : "User";

                return View("TransactionDefaultView", currentUser);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Unexpected error: {ex.Message}";
                return View("TransactionErrorView", new User());
            }
        }

        //// POST: /TransactionHistory/FilterTransactions
        //[HttpPost]
        //public IActionResult FilterTransactions(DateTime fromDate, DateTime toDate)
        //{
        //    try
        //    {
        //        var username = GetLoggedInUsername();
        //        if (string.IsNullOrEmpty(username))
        //            return RedirectToAction("GetDefaultView", "Login");

        //        // Get current user
        //        var userClient = new RestClient(userApiBaseUrl);
        //        var userRequest = new RestRequest("", Method.Get);
        //        var userResponse = userClient.Execute(userRequest);

        //        if (!userResponse.IsSuccessful)
        //            return View("TransactionErrorView", new User());

        //        var users = JsonConvert.DeserializeObject<List<User>>(userResponse.Content);
        //        var currentUser = users?.FirstOrDefault(u => u.Username == username);

        //        if (currentUser == null)
        //            return RedirectToAction("GetDefaultView", "Login");

        //        // Pass date range to view
        //        ViewBag.FromDate = fromDate;
        //        ViewBag.ToDate = toDate;
        //        TempData["UserRole"] = currentUser.isAdmin ? "Admin" : "User";

        //        return View("TransactionFilteredView", currentUser);
        //    }
        //    catch
        //    {
        //        return View("TransactionErrorView", new User());
        //    }
        //}

        [HttpPost]
        public IActionResult FilterTransactions(int accountId, DateTime fromDate, DateTime toDate, string sortOrder = "newest")
        {
            try
            {
                var username = GetLoggedInUsername();
                if (string.IsNullOrEmpty(username))
                    return RedirectToAction("GetDefaultView", "Login");

                // Get current user
                var userClient = new RestClient(userApiBaseUrl);
                var userRequest = new RestRequest("", Method.Get);
                var userResponse = userClient.Execute(userRequest);

                if (!userResponse.IsSuccessful)
                    return View("TransactionErrorView", new User());

                var users = JsonConvert.DeserializeObject<List<User>>(userResponse.Content);
                var currentUser = users?.FirstOrDefault(u => u.Username == username);

                if (currentUser == null)
                    return RedirectToAction("GetDefaultView", "Login");

                // Get transactions
                var transactionClient = new RestClient(businessApiBaseUrl);
                var transactionRequest = new RestRequest("", Method.Get);
                var transactionResponse = transactionClient.Execute(transactionRequest);

                List<Transaction> userTransactions = new List<Transaction>();

                if (transactionResponse.IsSuccessful && !string.IsNullOrEmpty(transactionResponse.Content))
                {
                    var settings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        MaxDepth = 2
                    };

                    var allTransactions = JsonConvert.DeserializeObject<List<Transaction>>(transactionResponse.Content, settings);

                    if (allTransactions != null)
                    {
                        // Filter transactions for the selected account
                        userTransactions = allTransactions
                            .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                            .ToList();
                    }
                }

                // Pass filter parameters to view
                ViewBag.UserTransactions = userTransactions;
                ViewBag.FromDateFilter = fromDate;
                ViewBag.ToDateFilter = toDate;
                ViewBag.SortOrder = sortOrder;

                TempData["UserRole"] = currentUser.isAdmin ? "Admin" : "User";
                return View("TransactionDefaultView", currentUser);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error filtering transactions: {ex.Message}";
                return View("TransactionErrorView", new User());
            }
        }

        // Error view
        [HttpGet("error")]
        public IActionResult TransactionErrorView() => View("TransactionErrorView", new User());
    }
}