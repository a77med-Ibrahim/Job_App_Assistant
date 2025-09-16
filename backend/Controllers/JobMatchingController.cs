using System.Text;
using System.Text.Json;
using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("/JobMatching/[controller]")]
    public class JobMatchingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public JobMatchingController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }
        [HttpGet("SaveJobDescription")]
        public Task<IActionResult> SaveJobDescription(string jobDescription)
        {
            var description = _context.
            return
        }
        
        public async Task<string> GradingAsync(string jobDescription)
        {
            try
            {
                var apiKey = await _context.ApiKeys.Select(k => k.Key).FirstOrDefaultAsync();
                var shortResume = await _context.Resumes.Select(s => s.ShortResume).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(apiKey)) return string.Empty;
                var prompt = $@"
                You will be getting a job descrption, and some keywords and sentences, using the keysentences, and keywords,
                I want you to give a score out of 100 on how these keywords or keysentences match the job description.
                This is where you can find the keywords:{shortResume}  and here you can find the job description {jobDescription}";

                var requestBody = new
                {
                    contents = new[]
                {
                    new {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                    generationConfig = new
                    {
                        maxOutputTokens = 150
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}",
                content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Gemini API error:" + errorBody);
                    return string.Empty;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseBody);

                Console.WriteLine("Gemini raw response: " + responseBody);

                var candidates = document.RootElement.GetProperty("candidates");
                if (candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var contentElement))
                    {
                        if (contentElement.TryGetProperty("parts", out var parts) &&
                            parts.ValueKind == JsonValueKind.Array && parts.GetArrayLength() > 0)
                        {
                            return parts[0].GetProperty("text").GetString() ?? string.Empty;
                        }
                    }
                }
                Console.WriteLine("⚠️ No text found in Gemini response: " + responseBody);
                return string.Empty;


            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating short resume: " + ex);
                return string.Empty;
            }
        }
    }
}