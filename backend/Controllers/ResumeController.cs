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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public ResumeController(AppDbContext context, IHttpClientFactory httpClientFactory, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _scopeFactory = scopeFactory;
        }


        [HttpPost("UpsertResume")]
        public async Task<IActionResult> UpsertResume([FromBody] string resumeText)
        {
            if (string.IsNullOrWhiteSpace(resumeText))
            {
                return BadRequest(new { Message = "Resume text cannot be empty" });
            }

            Resume targetResume;
            var existingResume = await _context.Resumes.Include(r => r.Record).FirstOrDefaultAsync();

            if (existingResume != null) // If there is an existing record in the Resume table
            {
                existingResume.Text = resumeText;
                existingResume.EmbeddingJson = string.Empty; // Embeddings will be done in the background, that's why it's initially empty
                existingResume.Record.Date = DateTime.UtcNow;
                existingResume.Record.Description = "Resume";
                targetResume = existingResume;
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
            
            // Start background task for embeddings
            _ = Task.Run(async () => await GenerateEmbeddingsAsync(targetResume.Id));
            
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

        private async Task GenerateEmbeddingsAsync(int resumeId)
        {
    try
    {
        using var scope = _scopeFactory.CreateScope();
        var scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var resume = await scopedContext.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId);
        if (resume == null) return;

        var apiKey = await scopedContext.ApiKeys.Select(k => k.Key).FirstOrDefaultAsync();
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("No API key found");
            return;
        }

        var requestBody = new
        {
            model = "gemini-embedding-001",
            content = new
            {
                parts = new[]
                {
                    new { text = resume.Text }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync(
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={apiKey}", 
            content
        );

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Gemini API error: {response.StatusCode}");
            var errorBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Error body: " + errorBody);
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseBody);

        // Parse embeddings
        var embeddingArray = document.RootElement
            .GetProperty("embedding")
            .GetProperty("values")
            .EnumerateArray()
            .Select(x => x.GetDouble())
            .ToArray();

        resume.EmbeddingJson = JsonSerializer.Serialize(embeddingArray);
        await scopedContext.SaveChangesAsync();

        Console.WriteLine("Embeddings generated successfully!");
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error generating embeddings: {e}");
    }
}

    }
}