using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class ChatMessageRequest
    {
        [Required]
        public Guid ChatId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;
    }
}
