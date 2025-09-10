using Microsoft.AspNetCore.Mvc;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using System.Text.Json;
using System.Text;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public ResumeController(AppDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        [HttpPost("UpsertResume")]
        public async Task<IActionResult> UpSeartResume([FromBody] string resumeText)
        {
            Resume targetResume;
            var existingResume = await _context.Resumes.Include(r => r.Record).FirstOrDefaultAsync();

            if (existingResume != null) // If there is an existing record in the Resume table
            {
                existingResume.Text = resumeText;
                existingResume.EmbeddingJson = string.Empty; // Embeddings will be done in the background, that's why it's initally empty
                existingResume.Record.Date = DateTime.UtcNow;
                existingResume.Record.Description = "Resume";
                targetResume = existingResume; // TODO: Carry on from here
            }
            else
            {
                var record = new Record
                {
                    Date = DateTime.UtcNow,
                    Type = RecordType.Resume,
                    Description = "Resume"
                };
                var resume = new Resume
                {
                    Text = resumeText,
                    EmbeddingJson = string.Empty,
                    Record = record
                };
                _context.Records.Add(record);
                _context.Resumes.Add(resume);
                targetResume = resume;
            }

            await _context.SaveChangesAsync();
            await GenerateEmbeddingsAsync();
            return Ok(new { Message = "Resume uploaded successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> GetResume()
        {
            var resume = await _context.Resumes.Include(r => r.Record).FirstOrDefaultAsync();

            if (resume == null) return NotFound(new { Message = "No resume found" });

            return Ok(new
            {
                Id = resume.Id,
                Text = resume.Text,
                HasEmbeddings = !string.IsNullOrEmpty(resume.EmbeddingJson),
                UploadDate = resume.Record.Date,
                Description = resume.Record.Description

            });
        }

        private async Task GenerateEmbeddingsAsync()
        {
            try
            {
                var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == 1);

                if (resume != null)
                {
                    var apiKey = await _context.ApiKeys.Select(k => k.Key).FirstOrDefaultAsync();
                    if (apiKey != null)
                    {
                        // This part is used to make the text as Gemini expects
                        var requestBody = new
                        {
                            model = "models/text-embedding-001",
                            content = new
                            {
                                parts = new[]{
                                    new {text = resume.Text}
                                }
                            }
                        };
                        // This part is to convert the above part to json
                        var json = JsonSerializer.Serialize(requestBody);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        // Http request to gemini
                        var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/text-embedding-001:embedContent?key={apiKey}", content);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();

                            using var document = JsonDocument.Parse(responseBody);
                            var embeddingArray = document.RootElement.GetProperty("embedding").GetProperty("values").EnumerateArray()
                            .Select(x => x.GetSingle()).ToArray();

                            resume.EmbeddingJson = JsonSerializer.Serialize(embeddingArray);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            Console.WriteLine($"Gemini API error: {response.StatusCode}");
                        }
                              

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error generating embeddings: {e.Message}");
            }
            
        }
        
    }
}