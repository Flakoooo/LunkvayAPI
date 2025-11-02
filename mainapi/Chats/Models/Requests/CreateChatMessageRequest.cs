using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class CreateChatMessageRequest
    {
        [Required]
        public required Guid ChatId { get; init; }

        [Required]
        public required string Message { get; init; }
    }
}
