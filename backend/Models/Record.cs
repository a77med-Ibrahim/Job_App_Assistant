namespace backend.Models
{
    public class Record
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public RecordType Type { get; set; } //JobMatch or CoverLetter
        public string Description { get; set; }=string.Empty;

    }
}