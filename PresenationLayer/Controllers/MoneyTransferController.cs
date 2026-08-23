using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DCAssignment2.Models;

namespace PresentationLayer.Controllers
{
    [Route("MoneyTransfer")]
    public class MoneyTransferController : Controller
    {
        private const string businessTransactionApi = "http://localhost:5080/api/BusinessTransaction";
        private const string businessAccountApi = "http://localhost:5080/api/BusinessAccount";
        private const string businessUserApi = "http://localhost:5080/api/BusinessUser";

        // Helper to get logged-in username
        private string? GetLoggedInUsername() => Request.Cookies["SessionID"];

        // GET: Money transfer view
        [HttpGet("MoneyTransferDefaultView")]
        public IActionResult MoneyTransferDefaultView()
        {
            try
            {
                var username = GetLoggedInUsername();
                if (string.IsNullOrEmpty(username))
                    return RedirectToAction("GetDefaultView", "Login");

                // Get current user with accounts
                var userClient = new RestClient(businessUserApi);
                var userRequest = new RestRequest("", Method.Get);
                var userResponse = userClient.Execute(userRequest);

                if (!userResponse.IsSuccessful)
                {
                    ViewBag.ErrorMessage = "Error loading user data.";
                    return View("MoneyTransferErrorView");
                }

                var users = JsonConvert.DeserializeObject<List<User>>(userResponse.Content);
                var currentUser = users?.FirstOrDefault(u => u.Username == username);

                if (currentUser == null)
                    return RedirectToAction("GetDefaultView", "Login");

                // Get all accounts for recipient selection
                var accountClient = new RestClient(businessAccountApi);
                var accountRequest = new RestRequest("", Method.Get);
                var accountResponse = accountClient.Execute(accountRequest);

                if (accountResponse.IsSuccessful)
                {
                    var allAccounts = JsonConvert.DeserializeObject<List<Account>>(accountResponse.Content);
                    // Filter out current user's accounts for "other" recipient selection
                    var otherAccounts = allAccounts?
                        .Where(a => a.UserId != currentUser.Id)
                        .ToList() ?? new List<Account>();

                    ViewBag.OtherAccounts = otherAccounts;
                }

                TempData["UserRole"] = currentUser.isAdmin ? "Admin" : "User";
                return View("MoneyTransferDefaultView", currentUser);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading transfer page: {ex.Message}";
                return View("MoneyTransferErrorView");
            }
        }

        // POST: Perform money transfer
        [HttpPost("Transfer")]
        [ValidateAntiForgeryToken]
        public IActionResult Transfer(int FromAccountId, string Mode, int? RecipientAccountId, string RecipientAccountNumber, decimal Amount, string Description)
        {
            try
            {
                var username = GetLoggedInUsername();
                if (string.IsNullOrEmpty(username))
                    return RedirectToAction("GetDefaultView", "Login");

                // Get current user for re-display
                var userClient = new RestClient(businessUserApi);
                var userRequest = new RestRequest("", Method.Get);
                var userResponse = userClient.Execute(userRequest);

                if (!userResponse.IsSuccessful)
                {
                    ViewBag.ErrorMessage = "Error loading user data.";
                    return View("MoneyTransferErrorView");
                }

                var users = JsonConvert.DeserializeObject<List<User>>(userResponse.Content);
                var currentUser = users?.FirstOrDefault(u => u.Username == username);

                if (currentUser == null)
                    return RedirectToAction("GetDefaultView", "Login");

                // Get all accounts
                var accountClient = new RestClient(businessAccountApi);
                var accountRequest = new RestRequest("", Method.Get);
                var accountResponse = accountClient.Execute(accountRequest);

                if (!accountResponse.IsSuccessful)
                {
                    ViewBag.ErrorMessage = "Error loading accounts.";
                    ViewBag.OtherAccounts = new List<Account>();
                    return View("MoneyTransferDefaultView", currentUser);
                }

                var allAccounts = JsonConvert.DeserializeObject<List<Account>>(accountResponse.Content);
                var otherAccounts = allAccounts?.Where(a => a.UserId != currentUser.Id).ToList() ?? new List<Account>();
                ViewBag.OtherAccounts = otherAccounts;

                // Validate sender account
                var senderAccount = allAccounts?.FirstOrDefault(a => a.Id == FromAccountId);
                if (senderAccount == null)
                {
                    ViewBag.ErrorMessage = "Sender account not found.";
                    return View("MoneyTransferDefaultView", currentUser);
                }

                // Validate sender owns the account
                if (senderAccount.UserId != currentUser.Id)
                {
                    ViewBag.ErrorMessage = "You can only transfer from your own accounts.";
                    return View("MoneyTransferDefaultView", currentUser);
                }

                // Find recipient account based on mode
                Account recipientAccount = null;
                if (Mode == "own")
                {
                    if (RecipientAccountId.HasValue)
                    {
                        recipientAccount = allAccounts?.FirstOrDefault(a => a.Id == RecipientAccountId.Value);
                        if (recipientAccount?.UserId != currentUser.Id)
                        {
                            ViewBag.ErrorMessage = "Selected account does not belong to you.";
                            return View("MoneyTransferDefaultView", currentUser);
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Please select a recipient account.";
                        return View("MoneyTransferDefaultView", currentUser);
                    }
                }
                else if (Mode == "other")
                {
                    if (!string.IsNullOrWhiteSpace(RecipientAccountNumber))
                    {
                        recipientAccount = allAccounts?.FirstOrDefault(a =>
                            a.AccountNumber == RecipientAccountNumber.Trim());
                    }
                    else if (RecipientAccountId.HasValue)
                    {
                        recipientAccount = allAccounts?.FirstOrDefault(a => a.Id == RecipientAccountId.Value);
                    }

                    if (recipientAccount == null)
                    {
                        ViewBag.ErrorMessage = "Recipient account not found.";
                        return View("MoneyTransferDefaultView", currentUser);
                    }

                    // For other transfers, ensure recipient is different from sender
                    if (recipientAccount.UserId == currentUser.Id)
                    {
                        ViewBag.ErrorMessage = "For transfers to your own accounts, please use 'Own account' mode.";
                        return View("MoneyTransferDefaultView", currentUser);
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid transfer mode.";
                    return View("MoneyTransferDefaultView", currentUser);
                }

                if (recipientAccount == null)
                {
                    ViewBag.ErrorMessage = "Recipient account not found.";
                    return View("MoneyTransferDefaultView", currentUser);
                }

                // Prevent transferring to same account
                if (senderAccount.Id == recipientAccount.Id)
                {
                    ViewBag.ErrorMessage = "Cannot transfer to the same account.";
                    return View("MoneyTransferDefaultView", currentUser);
                }

                // Validate amount
                if (Amount <= 0)
                {
                    ViewBag.ErrorMessage = "Transfer amount must be greater than 0.";
                    return View("MoneyTransferDefaultView", currentUser);
                }

                if (senderAccount.Balance < Amount)
                {
                    ViewBag.ErrorMessage = "Insufficient funds in the source account.";
                    return View("MoneyTransferDefaultView", currentUser);
                }

                // Prepare transaction
                var transaction = new Transaction
                {
                    FromAccountId = senderAccount.Id,
                    ToAccountId = recipientAccount.Id,
                    AmountTransferred = Amount,
                    Timestamp = DateTime.Now,
                    Description = string.IsNullOrWhiteSpace(Description) ?
                        $"Transfer from {senderAccount.AccountNumber} to {recipientAccount.AccountNumber}" :
                        Description
                };

                // Call Business API to create transaction
                var transactionClient = new RestClient(businessTransactionApi);
                var transactionRequest = new RestRequest("", Method.Post);
                transactionRequest.AddJsonBody(transaction);
                var transactionResponse = transactionClient.Execute(transactionRequest);

                if (!transactionResponse.IsSuccessful)
                {
                    var errorContent = transactionResponse.Content ?? "Unknown error";
                    ViewBag.ErrorMessage = $"Failed to complete transaction: {errorContent}";
                    return View("MoneyTransferDefaultView", currentUser);
                }

                // Success - show success view with details
                ViewBag.SuccessMessage = "Transfer completed successfully!";
                ViewBag.FromAccountNumber = senderAccount.AccountNumber;
                ViewBag.RecipientAccountNumber = recipientAccount.AccountNumber;
                ViewBag.Amount = Amount.ToString("C");
                ViewBag.Description = transaction.Description;
                ViewBag.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ViewBag.TransactionId = JsonConvert.DeserializeObject<Transaction>(transactionResponse.Content)?.TransactionId;

                TempData["UserRole"] = currentUser.isAdmin ? "Admin" : "User";
                return View("MoneyTransferSuccessView", currentUser);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Unexpected error: {ex.Message}";
                return View("MoneyTransferErrorView");
            }
        }

        // GET: Success view
        [HttpGet("Success")]
        public IActionResult MoneyTransferSuccessView()
        {
            var username = GetLoggedInUsername();
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("GetDefaultView", "Login");

            var userClient = new RestClient(businessUserApi);
            var userRequest = new RestRequest("", Method.Get);
            var userResponse = userClient.Execute(userRequest);

            if (!userResponse.IsSuccessful)
                return RedirectToAction("GetDefaultView", "Login");

            var users = JsonConvert.DeserializeObject<List<User>>(userResponse.Content);
            var currentUser = users?.FirstOrDefault(u => u.Username == username);

            if (currentUser == null)
                return RedirectToAction("GetDefaultView", "Login");

            TempData["UserRole"] = currentUser.isAdmin ? "Admin" : "User";
            return View("MoneyTransferSuccessView", currentUser);
        }

        // GET: Error view
        [HttpGet("Error")]
        public IActionResult MoneyTransferErrorView()
        {
            var username = GetLoggedInUsername();
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("GetDefaultView", "Login");

            var userClient = new RestClient(businessUserApi);
            var userRequest = new RestRequest("", Method.Get);
            var userResponse = userClient.Execute(userRequest);

            if (!userResponse.IsSuccessful)
                return RedirectToAction("GetDefaultView", "Login");

            var users = JsonConvert.DeserializeObject<List<User>>(userResponse.Content);
            var currentUser = users?.FirstOrDefault(u => u.Username == username);

            if (currentUser == null)
                return RedirectToAction("GetDefaultView", "Login");

            TempData["UserRole"] = currentUser.isAdmin ? "Admin" : "User";
            return View("MoneyTransferErrorView", currentUser);
        }
    }
}