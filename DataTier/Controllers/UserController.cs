using DCAssignment2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace DataTier.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private const string dcApiBase = "http://localhost:5217/api/Users";

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(dcApiBase);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            var data = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<User>>(data);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync($"{dcApiBase}/{id}");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            var data = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<User>(data);
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] User user)
        {
            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(dcApiBase, content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            var created = await response.Content.ReadAsStringAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{dcApiBase}/{id}", content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            using var client = new HttpClient();
            var response = await client.DeleteAsync($"{dcApiBase}/{id}");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            return NoContent();
        }
    }
}
