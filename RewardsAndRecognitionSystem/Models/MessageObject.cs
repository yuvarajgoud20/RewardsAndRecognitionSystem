namespace RewardsAndRecognitionSystem.Models
{
    public class MessageObject
    {
        public string Message { get; set; }
        public string ErrorDescription { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }
}
