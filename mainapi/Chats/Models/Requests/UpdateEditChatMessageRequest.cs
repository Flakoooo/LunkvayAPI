using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class UpdateEditChatMessageRequest
    {
        [Required]
        public required Guid MessageId { get; init; }

        [Required]
        public required Guid ChatId { get; init; }

        [Required]
        public required string NewMessage { get; init; }
    }
}
