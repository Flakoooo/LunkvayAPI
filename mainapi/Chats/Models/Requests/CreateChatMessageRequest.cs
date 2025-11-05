using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class CreateChatMessageRequest
    {
        [Required]
        public required string Message { get; init; }

        public Guid? ChatId { get; init; }

        public Guid? ReceiverId { get; init; }
    }
}
