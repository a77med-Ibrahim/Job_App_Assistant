using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiKeyController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ApiKeyController(AppDbContext context)
        {
            _context = context;
        }

        // [HttpPost("PostApiKeys")]
        public async Task<IActionResult> SaveApiKey([FromBody] ApiKey request)
        {
            var apiKey = new ApiKey
            {
                Key = request.Key
            };
            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();
            return Ok(new { message = "API key saved successfully.", id = apiKey.Id });
        }
        [HttpGet("GetApiKeys")]
        public IActionResult GetApiKeys()
        {
            var apiKey = _context.ApiKeys.FirstOrDefault();
            if (apiKey == null)
            {
                return NotFound(new { message = "No API keys found." });
            }
            return Ok(new { key = apiKey.Key, id = apiKey.Id });
        }

        [HttpPut("UpdateApiKeys")]
        public async Task<IActionResult> UpdateApiKeys([FromBody] ApiKey request)
        {
            var existingApiKey = _context.ApiKeys.FirstOrDefault();
            if (existingApiKey == null) {
            var apiKey = new ApiKey {Key = request.Key};
            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();
            return Ok(new { message = "API key saved successfully.", id = apiKey.Id });
            }
            _context.ApiKeys.Update();
            return Ok("Updated succesfully");
        }
        

    }
}