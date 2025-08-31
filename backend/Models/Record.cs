namespace backend.Models
{
    public class Record
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } //JobMatch or CoverLetter
        public string Description { get; set; }

        //Navigation properties
        public JobMatch JobMatch { get; set; }
        public CoverLetter CoverLetter { get; set; }
    }
}