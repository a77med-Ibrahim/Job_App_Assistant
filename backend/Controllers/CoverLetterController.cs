using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoverLetterController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        public CoverLetterController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }
        public class CoverLetterRequestDto
        {
            public string JobDescription { get; set; }
            public string additionalDetails { get; set; }
        }
        [HttpGet("GetCoverLetter")]
        public async Task<IActionResult> GetCoverLetter()
        {
            var coverLetter = await _context.CoverLetters.OrderByDescending(c => c.Id).Select(c => c.CoverLetterText).FirstOrDefaultAsync();
            return Ok(new { coverLetter });
        }

        [HttpPost("CoverLetter")]
        [Consumes("text/plain", "application/json")]
        public async Task<IActionResult> CoverLetterGenerator([FromBody] CoverLetterRequestDto request)
        {
            var jobDescription = request.JobDescription;
            var additionalDetails = request.additionalDetails;

            var record = new Record
            {
                Date = DateTime.UtcNow,
                Type = RecordType.CoverLetter,
                Description = "CoverLetter"
            };
            var coverLetter = new CoverLetter
            {
                CoverLetterText = await GeneratingCoverLetterAsync(jobDescription, additionalDetails),
                Record = record,
            };
            _context.Records.Add(record);
            _context.CoverLetters.Add(coverLetter);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cover letter generated successfully", coverLetter = coverLetter.CoverLetterText });
        }
        public async Task<string> GeneratingCoverLetterAsync(string jobDescription, string additionalDetails)
        {
            try
            {
                var apiKey = await _context.ApiKeys.Select(k => k.Key).FirstOrDefaultAsync();
                var shortResume = await _context.Resumes.Select(s => s.ShortResume).FirstOrDefaultAsync();
                var resume = await _context.Resumes.Select(s => s.Text).FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(apiKey)) return "No API key found";

                var prompt = $@"
                You will be getting a job description, {jobDescription}, and a short resume, {shortResume},
                and an additional information, {additionalDetails},
                you are a professioanl cover letter writer, I want you using the above details to write a very strong
                cover letter which is very eye catching, very human like, and using easy English wording to write a cover letter
                that will make everyone accept the candidate.
                Important:
                Don't even add ** to bold a word
                Always use Dear Sir/Madam, don't include [Date],your address, etc or any extra field
                ";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new {
                            parts = new[] {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        maxOutputTokens = 7000,
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
                    Console.WriteLine("Gemini API error: " + errorBody);
                    return "Gemini API error: " + errorBody;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseBody) || responseBody.Contains("error"))
                {
                    Console.WriteLine("Invalid response from Gemini: " + responseBody);
                    return "Invalid response from Gemini: " + responseBody;
                }

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
                        if (!string.IsNullOrEmpty(text))
                        {
                            return text;
                        }
                    }
                }

                Console.WriteLine("No text found in Gemini response: " + responseBody);
                return "No text found in Gemini response.";


            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in GeneratingCoverLetterAsync: " + ex.Message);
                return "Error while generating cover letter: " + ex.Message;
            }
        }
    }
}