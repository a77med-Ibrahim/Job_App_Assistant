using Microsoft.AspNetCore.Mvc;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using backend.Models;

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

        [HttpPost("UpsertResume")]
        public async Task<IActionResult> UpSeartResume([FromBody] string resumeText)
        {

            var existingResume = await _context.Resumes.Include(r => r.Record).FirstOrDefaultAsync();

            if (existingResume != null) // If there is an existing record in the Resume table
            {
                existingResume.Text = resumeText;
                existingResume.EmbeddingJson = string.Empty; // Embeddings will be done in the background, that's why it's initally empty
                existingResume.Record.Date = DateTime.UtcNow;
                existingResume.Record.Description = "Resume";
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
            }
        }
        
    }
}