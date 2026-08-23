using DCAssignment2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace DataTier.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private const string dcApiBase = "http://localhost:5217/api/Transactions";

        [HttpGet]
        public async Task<IActionResult> GetTransactions()
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(dcApiBase);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            var data = await response.Content.ReadAsStringAsync();
            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(data);
            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(int id)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync($"{dcApiBase}/{id}");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            var data = await response.Content.ReadAsStringAsync();
            var transaction = JsonConvert.DeserializeObject<Transaction>(data);
            return Ok(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> PostTransaction([FromBody] Transaction transaction)
        {
            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(transaction), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(dcApiBase, content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            var created = await response.Content.ReadAsStringAsync();
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.TransactionId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] Transaction transaction)
        {
            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(transaction), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{dcApiBase}/{id}", content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            using var client = new HttpClient();
            var response = await client.DeleteAsync($"{dcApiBase}/{id}");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting DCAssignment2 API");

            return NoContent();
        }
    }
}
