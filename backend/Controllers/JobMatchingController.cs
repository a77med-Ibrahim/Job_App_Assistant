using System.Text;
using System.Text.Json;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("JobMatching/[controller]")]
    public class JobMatchingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        public class JobDescriptionDto
        {
            public string JobDescription{ get; set; }
        }

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
        [Consumes("text/plain","application/json")]
        public async Task<IActionResult> SaveJobDescription([FromBody] JobDescriptionDto dto)
        {
            var jobDescription = dto.JobDescription;

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
            return Ok(new { Message = "Score generated successfully.", Score = JobMatch.Score });
        }

        public async Task<int> GradingAsync(string jobDescription)
        {
            try
            {
                var apiKey = await _context.ApiKeys.Select(k => k.Key).FirstOrDefaultAsync();
                var shortResume = await _context.Resumes.Select(s => s.ShortResume).FirstOrDefaultAsync();
                Console.WriteLine("ShortResume: " + shortResume);
                Console.WriteLine("JobDescription: " + jobDescription);
                if (string.IsNullOrEmpty(apiKey)) return 0;


                var prompt = $@"
                Compare this resume with the job description and return a matching score from 0 to 100.
                Criteria to compare:
                - Job location vs the current applicant location, (if available) (20 points) give 10 points if not mentioned either by the user or the job description,
                - If he has experince with the same tools required by the job description (30 points),
                - Years of experince (30 points),
                - Lanugage asked by the job description and candidates skills in that language (20 points)
                
                Return ONLY the number, no explination.

                Resume: {shortResume}
                Job Description: {jobDescription}
                ";

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
                        maxOutputTokens = 3500,
                        temperature = 0.7,        
                        topP = 0.8  
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
            
                if (string.IsNullOrEmpty(responseBody) || responseBody.Contains("error"))
                {
                    Console.WriteLine(" Invalid response from Gemini: " + responseBody);
                    return 0;
                }    
                
                using var document = JsonDocument.Parse(responseBody);
                var fullJson = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("Parsed JSON structure: " + fullJson);

                if (document.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.ValueKind == JsonValueKind.Array && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];

                    if (firstCandidate.TryGetProperty("content", out var contentElement) &&
                        contentElement.TryGetProperty("parts", out var parts) &&
                        parts.ValueKind == JsonValueKind.Array && parts.GetArrayLength() > 0)
                    {
                        var text = parts[0].GetProperty("text").GetString();
                        if (string.IsNullOrEmpty(text))
                        {
                            Console.WriteLine("No text content in Gemini response");
                            return 0;
                        }
                        var cleanedText = System.Text.RegularExpressions.Regex.Match(text, @"\d+");


                        if (cleanedText.Success && int.TryParse(cleanedText.Value, out int score))
                        {
                            return Math.Max(0, Math.Min(100, score)); 
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