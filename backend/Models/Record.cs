namespace backend.Models
{
    public class Record
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public RecordType Type { get; set; } //JobMatch, CoverLetter, or resume
        public string Description { get; set; }=string.Empty; //Cover letter for Microsoft internship application OR Software Developer position at Google - good match"

    }
}