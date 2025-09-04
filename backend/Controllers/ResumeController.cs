using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResumeController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/resume
        [HttpPost("Resume")]
        public async Task<IActionResult> UploadResume([FromBody] ResumeUploadRequest request)
        {
            var existingResume = _context.Resumes
                .FirstOrDefault(r => r.Record.Description == request.UserId);

            if (existingResume != null)
            {
                existingResume.Text = request.Text;
                existingResume.EmbeddingJson = JsonSerializer.Serialize(request.Embedding);
                existingResume.Record.Date = DateTime.UtcNow;
            }
            else
            {
                var record = new Record
                {
                    Date = DateTime.UtcNow,
                    Type = RecordType.Resume,
                    Description = request.UserId
                };

                var resume = new Resume
                {
                    Text = request.Text,
                    EmbeddingJson = JsonSerializer.Serialize(request.Embedding),
                    Record = record
                };

                _context.Records.Add(record);
                _context.Resumes.Add(resume);
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Resume saved successfully" });
        }
        [HttpPost]
        public async Task<IActionResult> UploadResume([FromForm] IFormFile resume)
        {
            if (resume == null || resume.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

             var filePath = Path.Combine("Uploads", resume.FileName);

             using (var stream = new FileStream(filePath, FileMode.Create))
        {
        await resume.CopyToAsync(stream);
        }

    return Ok(new { message = "File uploaded successfully" });
}
    }
    
    public class ResumeUploadRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;  // extracted resume text
        public float[] Embedding { get; set; } = Array.Empty<float>(); // embedding vector
    }
}