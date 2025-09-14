using backend.Data;
using Microsoft.AspNetCore.Mvc;

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

        // public async Task InputEmbbedings()
        // {
            
        // }
    }
}