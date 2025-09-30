using Microsoft.AspNetCore.Mvc;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using System.Text.Json;
using System.Text;

namespace backend.Controllers
{
    [ApiController]
    [Route("Resume/[controller]")]
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

            if (existingResume != null) 
            {
                existingResume.Text = resumeText;
                existingResume.EmbeddingJson = string.Empty;
                existingResume.ShortResume = await GenerateShortResumeAsync(resumeText);
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
                    ShortResume = await GenerateShortResumeAsync(resumeText),
                    Record = record
                };
                _context.Records.Add(record);
                _context.Resumes.Add(resume);
                targetResume = resume;
            }

            await _context.SaveChangesAsync();
            targetResume.ShortResume = await GenerateShortResumeAsync(resumeText);
            await _context.SaveChangesAsync();

            // Start background task for embeddings
            // _ = Task.Run(async () => await GenerateResumeEmbeddingsAsync(targetResume.Id));

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
        //Deactivated becasue I changed the approach
        private async Task GenerateResumeEmbeddingsAsync(int resumeId)
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

        [HttpGet("IsResumeRecordEmpty")]
        public async Task<IActionResult> IsThereAnExistingResume()
        {
            var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == 1);

            if (resume == null) return Ok(new { isSaved = false });

            return Ok(new { isSaved = true, id = resume.Id });
        }

        public async Task<string> GenerateShortResumeAsync(string resumeText)
        {
            try
            {
                var apiKey = await _context.ApiKeys.Select(k => k.Key).FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(apiKey)) return string.Empty;
                var prompt = $@"
            You are a professional keyword and information extractor for resumes.
            Your task is to read a candidate's resume and produce a **concise, structured summary in 3–4 sentences** containing only the most relevant details:

            - Work experience (roles, years)
            - Skills used in projects or experience
            - Education (Include only the degree and field)
            - Location (Very important)

            ⚡ Ignore: Contact info, hobbies, soft skills unrelated to technical work, standalone Skills sections, comapny names, university name, person name.

            Example output:
            '3+ years experience as Full-Stack Developer at Temo e.k (Frankfurt) and Marmara Centre (Istanbul), AI Engineer Intern at 180 Degree. Skilled in ASP.NET Core, React JS, Tailwind CSS, Entity Framework, CI/CD, Docker, LangChain, and RAG pipelines. Bachelor’s in Computer Engineering from Istanbul Bilgi University. Based in Frankfurt and Istanbul.'

            Now extract a short summary from this resume:{ resumeText} ";
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
                    maxOutputTokens = 2500
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
                Console.WriteLine(" No text found in Gemini response: " + responseBody);
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