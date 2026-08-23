using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DCAssignment2.Models;

namespace BusinessWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessTransactionController : ControllerBase
    {
        // ✅ FIXED: Correct base URL to match Data Tier controller
        private const string dataApiBaseUrl = "http://localhost:5217/api/Transactions";

        // GET: api/BusinessTransaction
        [HttpGet]
        public IActionResult GetTransactions()
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(response.Content ?? "[]");
            return Ok(transactions);
        }

        // GET: api/BusinessTransaction/5
        [HttpGet("{id:int}")]
        public IActionResult GetTransaction(int id)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"{id}", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var transaction = JsonConvert.DeserializeObject<Transaction>(response.Content ?? "{}");
            return Ok(transaction);
        }

        //// POST: api/BusinessTransaction
        //[HttpPost]
        //public IActionResult CreateTransaction([FromBody] Transaction transaction)
        //{
        //    var client = new RestClient(dataApiBaseUrl);
        //    var request = new RestRequest("", Method.Post);
        //    request.AddJsonBody(transaction);
        //    var response = client.Execute(request);

        //    if (!response.IsSuccessful)
        //        return StatusCode((int)response.StatusCode, response.Content);

        //    return Ok(JsonConvert.DeserializeObject<Transaction>(response.Content ?? "{}"));
        //}

        [HttpPost]
        public IActionResult CreateTransaction([FromBody] TransactionCreateDTO transactionDto)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest("", Method.Post);
            request.AddJsonBody(transactionDto);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            return Ok(JsonConvert.DeserializeObject<Transaction>(response.Content ?? "{}"));
        }

        // PUT: api/BusinessTransaction/5
        [HttpPut("{id:int}")]
        public IActionResult UpdateTransaction(int id, [FromBody] Transaction transaction)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"{id}", Method.Put);
            request.AddJsonBody(transaction);
            var response = client.Execute(request);

            return StatusCode((int)response.StatusCode, response.Content);
        }

        // DELETE: api/BusinessTransaction/5
        [HttpDelete("{id:int}")]
        public IActionResult DeleteTransaction(int id)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"{id}", Method.Delete);
            var response = client.Execute(request);

            return StatusCode((int)response.StatusCode, response.Content);
        }

        // ✅ Added: Get transactions by account ID (matches data-tier endpoint)
        // GET: api/BusinessTransaction/account/123
        [HttpGet("account/{accountId:int}")]
        public IActionResult GetTransactionsByAccount(int accountId)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"account/{accountId}", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(response.Content ?? "[]");
            return Ok(transactions);
        }

        // ✅ Added: Get transactions by user ID (matches data-tier endpoint)
        // GET: api/BusinessTransaction/user/5
        [HttpGet("user/{userId:int}")]
        public IActionResult GetTransactionsByUser(int userId)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"user/{userId}", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(response.Content ?? "[]");
            return Ok(transactions);
        }

        // ✅ Added: Deposit endpoint
        // POST: api/BusinessTransaction/deposit?accountId=1&amount=100
        [HttpPost("deposit")]
        public IActionResult Deposit([FromQuery] int accountId, [FromQuery] decimal amount)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"deposit?accountId={accountId}&amount={amount}", Method.Post);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var result = JsonConvert.DeserializeObject<object>(response.Content ?? "{}");
            return Ok(result);
        }

        // ✅ Added: Withdraw endpoint
        // POST: api/BusinessTransaction/withdraw?accountId=1&amount=50
        [HttpPost("withdraw")]
        public IActionResult Withdraw([FromQuery] int accountId, [FromQuery] decimal amount)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"withdraw?accountId={accountId}&amount={amount}", Method.Post);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var result = JsonConvert.DeserializeObject<object>(response.Content ?? "{}");
            return Ok(result);
        }

        // ✅ Added: Test relationships endpoint
        // GET: api/BusinessTransaction/test-relationships
        [HttpGet("test-relationships")]
        public IActionResult TestTransactionRelationships()
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest("test-relationships", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var result = JsonConvert.DeserializeObject<object>(response.Content ?? "{}");
            return Ok(result);
        }
    }
}
