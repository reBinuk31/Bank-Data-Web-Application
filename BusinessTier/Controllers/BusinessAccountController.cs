using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DCAssignment2.Models;

namespace BusinessWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessAccountController : ControllerBase
    {
        private const string dataApiBaseUrl = "http://localhost:5217/api/Accounts";

        [HttpGet]
        public IActionResult GetAccounts()
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var accounts = JsonConvert.DeserializeObject<List<Account>>(response.Content);
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public IActionResult GetAccount(int id)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"{id}", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var account = JsonConvert.DeserializeObject<Account>(response.Content);
            return Ok(account);
        }

        [HttpPost]
        public IActionResult CreateAccount([FromBody] Account account)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest("", Method.Post);
            request.AddJsonBody(account);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            return Ok(JsonConvert.DeserializeObject<Account>(response.Content));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateAccount(int id, [FromBody] Account account)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"{id}", Method.Put);
            request.AddJsonBody(account);
            var response = client.Execute(request);

            return StatusCode((int)response.StatusCode, response.Content);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAccount(int id)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"{id}", Method.Delete);
            var response = client.Execute(request);

            return StatusCode((int)response.StatusCode, response.Content);
        }

        [HttpGet("user/{userId:int}")]
        public IActionResult GetAccountsByUser(int userId)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"user/{userId}", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var accounts = JsonConvert.DeserializeObject<List<Account>>(response.Content ?? "[]");
            return Ok(accounts);
        }


        [HttpGet("number/{accountNumber}")]
        public IActionResult GetAccountByNumber(string accountNumber)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"number/{accountNumber}", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var account = JsonConvert.DeserializeObject<Account>(response.Content ?? "{}");
            return Ok(account);
        }


        [HttpGet("test-relationships")]
        public IActionResult TestAccountRelationships()
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

