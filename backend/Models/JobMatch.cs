namespace backend.Models
{
        public class JobMatch
        {
            public int Id { get; set; }
            public int Score { get; set; }
            public int RecordId { get; set; }
            public Record Record { get; set; } = null!;
        }
}
