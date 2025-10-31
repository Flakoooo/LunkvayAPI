using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class ChatMessageRequest
    {
        [Required]
        public required Guid ChatId { get; set; }

        [Required]
        public required string Message { get; set; }
    }
}
