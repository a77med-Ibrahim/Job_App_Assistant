namespace backend.Models
{
    public class CoverLetter
    {
        public int Id { get; set; }
        public string CoverLetterText { get; set; } = string.Empty;
        public int RecordId { get; set; }
        public Record Record { get; set; } = null!;
    }
}