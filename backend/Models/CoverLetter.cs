namespace backend.Models
{
    public class CoverLetter
    {
        public int Id { get; set; }
        required public string CoverLetterText { get; set; }
        public int RecordId { get; set; }
        public Record Record { get; set; } = null!;
    }
}