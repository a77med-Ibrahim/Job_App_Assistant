using Microsoft.AspNetCore.Routing.Constraints;

namespace backend.Models
{
    public class Resume
    {
        public int Id { get; set; }
        public String Text { get; set; } = string.Empty;
        public String ShortResume { get; set; } = string.Empty;
        public String EmbeddingJson { get; set; } = string.Empty;
        public int RecordId { get; set; }
        public Record Record { get; set; } = null!;
    }
}