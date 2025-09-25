using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("CoverLetter/[controller]")]
    public class CoverLetterController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        public class JobDescriptionDto
        {
            public string JobDescription { get; set; }
        }
        public class additionalDetailsDto
        {
            public string additionalDetails { get; set; }
        }

        [HttpPost("CoverLetter")]
        [Consumes("text/plain", "application/json")]
        public async Task<IActionResult> CoverLetterGenerator([FromBody] JobDescriptionDto jobDto, [FromBody] additionalDetailsDto addDetailsDto)
        {
            var jobDescription = jobDto.JobDescription;
            var additionalDetails = addDetailsDto.additionalDetails;

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
        public async Task<string> GeneratingCoverLetterAsync(string jobDescription, string additionalDetails) {
            try
            {
                
            }
            catch
            {
                
            }
        }
    }
}