using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DCAssignment2.Models;

namespace BusinessWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessUserController : ControllerBase
    {
        private const string dataApiBaseUrl = "http://localhost:5217/api/Users";

        [HttpGet]
        public IActionResult GetUsers()
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest("", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var users = JsonConvert.DeserializeObject<List<User>>(response.Content);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"{id}", Method.Get);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            var user = JsonConvert.DeserializeObject<User>(response.Content);
            return Ok(user);
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest("", Method.Post);
            request.AddJsonBody(user);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
                return StatusCode((int)response.StatusCode, response.Content);

            return Ok(JsonConvert.DeserializeObject<User>(response.Content));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User user)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"{id}", Method.Put);
            request.AddJsonBody(user);
            var response = client.Execute(request);

            return StatusCode((int)response.StatusCode, response.Content);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var client = new RestClient(dataApiBaseUrl);
            var request = new RestRequest($"{id}", Method.Delete);
            var response = client.Execute(request);

            return StatusCode((int)response.StatusCode, response.Content);
        }
    }
}

