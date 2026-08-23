using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DCAssignment2.Models;

namespace PresentationLayer.Controllers
{
    [Route("TransactionManagement")]
    public class TransactionManagementController : Controller
    {
        private const string businessApiBaseUrl = "http://localhost:5080/api/BusinessTransaction";

        // Default view: show all transactions
        [HttpGet("TransactionDefaultView")]
        public IActionResult TransactionDefaultView(string sortBy = "Date", string order = "desc")
        {
            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return View("TransactionErrorView", new List<Transaction>());

            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(response.Content);

            transactions = SortTransactions(transactions, sortBy, order);

            return View("TransactionDefaultView", transactions);
        }

        // Filtered transactions view
        [HttpGet("TransactionFilteredView")]
        public IActionResult TransactionFilteredView(
            string searchUsername,
            string searchAccount,
            string searchDescription,
            string fromDate,
            string toDate,
            string sortBy = "Date",
            string order = "desc")
        {
            var client = new RestClient(businessApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return View("TransactionErrorView", new List<Transaction>());

            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(response.Content);

            // Filter by username, account, description
            transactions = transactions.Where(t =>
                (string.IsNullOrEmpty(searchUsername) ||
                    t.FromAccount?.User?.Username.Contains(searchUsername, StringComparison.OrdinalIgnoreCase) == true ||
                    t.ToAccount?.User?.Username.Contains(searchUsername, StringComparison.OrdinalIgnoreCase) == true) &&
                (string.IsNullOrEmpty(searchAccount) ||
                    t.FromAccount?.AccountNumber.Contains(searchAccount, StringComparison.OrdinalIgnoreCase) == true ||
                    t.ToAccount?.AccountNumber.Contains(searchAccount, StringComparison.OrdinalIgnoreCase) == true) &&
                (string.IsNullOrEmpty(searchDescription) ||
                    t.Description.Contains(searchDescription, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // Filter by date range
            if (DateTime.TryParse(fromDate, out DateTime fromDt))
                transactions = transactions.Where(t => t.Timestamp >= fromDt).ToList();
            if (DateTime.TryParse(toDate, out DateTime toDt))
                transactions = transactions.Where(t => t.Timestamp <= toDt).ToList();

            transactions = SortTransactions(transactions, sortBy, order);

            // Pass filter values back to view for form
            ViewBag.SearchUsername = searchUsername;
            ViewBag.SearchAccount = searchAccount;
            ViewBag.SearchDescription = searchDescription;
            ViewBag.FromDateStr = fromDate;
            ViewBag.ToDateStr = toDate;

            return View("TransactionFilteredView", transactions);
        }

        // Utility: sort transactions
        private List<Transaction> SortTransactions(List<Transaction> transactions, string sortBy, string order)
        {
            bool ascending = order?.ToLower() == "asc";

            return sortBy?.ToLower() switch
            {
                "amount" => ascending ? transactions.OrderBy(t => t.AmountTransferred).ToList()
                                      : transactions.OrderByDescending(t => t.AmountTransferred).ToList(),
                "date" or _ => ascending ? transactions.OrderBy(t => t.Timestamp).ToList()
                                         : transactions.OrderByDescending(t => t.Timestamp).ToList()
            };
        }
    }
}
