using System.Text;
using System.Text.Json;
using backend.Data;
using backend.Models;
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

        [HttpGet("GetScore")]
        public async Task<IActionResult> GetScore()
        {
            var score = await _context.JobMatches.OrderByDescending(s => s.Id).Select(s => s.Score).FirstOrDefaultAsync();
            return Ok(new{score});
        }

        [HttpPost("SaveJobDescription")]
        public async Task<IActionResult> SaveJobDescription([FromBody] string jobDescription)
        {

            var record = new Record
            {
                Date = DateTime.UtcNow,
                Type = RecordType.JobMatch,
                Description = "JobMatch"
            };
            var JobMatch = new JobMatch
            {
                Score = await GradingAsync(jobDescription),
                Text = jobDescription,
                EmbeddingJson = string.Empty,
                Record = record,
            };
            _context.Records.Add(record);
            _context.JobMatches.Add(JobMatch);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Score generated successfully." , Score = JobMatch.Score});
        }

        public async Task<int> GradingAsync(string jobDescription)
        {
            try
            {
                var apiKey = await _context.ApiKeys.Select(k => k.Key).FirstOrDefaultAsync();
                var shortResume = await _context.Resumes.Select(s => s.ShortResume).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(apiKey)) return 0;

                var prompt = $@"
                You are a professional resume grader. Your task is to compare a candidate's resume keywords and key sentences with a job description and give a **single numeric score out of 100** based on these criteria:

                1. Location (20 points): Does the candidate’s location match or is close to the job location?
                2. Tools / Technologies (40 points): How many of the tools, software, frameworks, or languages mentioned in the job description are found in the resume?
                3. Years of Experience (30 points): How well does the candidate's experience duration match the job requirements?
                4. Keywords / Key Sentences (10 points): How relevant are the candidate’s other key skills, roles, or sentences to the job description?

                Return **only a single integer** (0–100) without any explanation or extra text.

                Candidate Resume Keywords / Key Sentences: {shortResume}
                Job Description: {jobDescription}";

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
                    Console.WriteLine(" Gemini API error: " + errorBody);
                    return 0;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(" Gemini raw response: " + responseBody);

                using var document = JsonDocument.Parse(responseBody);

                if (document.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.ValueKind == JsonValueKind.Array && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];

                    if (firstCandidate.TryGetProperty("content", out var contentElement) &&
                        contentElement.TryGetProperty("parts", out var parts) &&
                        parts.ValueKind == JsonValueKind.Array && parts.GetArrayLength() > 0)
                    {
                        var text = parts[0].GetProperty("text").GetString();

                        if (int.TryParse(text, out int score))
                        {
                            return score;
                        }
                        else
                        {
                            Console.WriteLine(" Could not parse Gemini response to int. Raw text: " + text);
                            return 0;
                        }
                    }
                }

                Console.WriteLine(" No text found in Gemini response: " + responseBody);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating score: " + ex);
                return 0;
            }
        }
        

    }
}