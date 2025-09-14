namespace backend.Models
{
        public class JobMatch
        {
            public int Id { get; set; }
            public int Score { get; set; }
            public String Text { get; set; } = string.Empty;
            public String EmbeddingJson { get; set; } = string.Empty;
            public int RecordId { get; set; }
            public Record Record { get; set; } = null!;
        }
}
