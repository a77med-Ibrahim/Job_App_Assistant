using Microsoft.AspNetCore.Mvc;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using backend.Models;
using System.Net;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

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
            }
           
            await _context.SaveChangesAsync();
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
        
    }
}