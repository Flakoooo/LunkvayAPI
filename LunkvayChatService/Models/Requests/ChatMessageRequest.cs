namespace LunkvayAPI.src.Models.Requests
{
    public class ChatMessageRequest
    {
        public Guid ChatId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
