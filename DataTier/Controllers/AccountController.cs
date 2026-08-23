using DCAssignment2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace DataTier.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private const string dcApiBase = "http://localhost:5217/api/Accounts";

        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(dcApiBase);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            var data = await response.Content.ReadAsStringAsync();
            var accounts = JsonConvert.DeserializeObject<List<Account>>(data);
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(int id)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync($"{dcApiBase}/{id}");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            var data = await response.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<Account>(data);
            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> PostAccount([FromBody] Account account)
        {
            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(account), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(dcApiBase, content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            var created = await response.Content.ReadAsStringAsync();
            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] Account account)
        {
            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(account), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{dcApiBase}/{id}", content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            using var client = new HttpClient();
            var response = await client.DeleteAsync($"{dcApiBase}/{id}");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            return NoContent();
        }
    }
}
