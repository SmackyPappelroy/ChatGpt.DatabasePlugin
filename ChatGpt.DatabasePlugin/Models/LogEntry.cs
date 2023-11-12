namespace ChatGpt.WebApi.Models
{
    public class LogEntry
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime LogTime { get; set; }
        public float NumericValue { get; set; }
    }
}
