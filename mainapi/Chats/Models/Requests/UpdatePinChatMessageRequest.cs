using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class UpdatePinChatMessageRequest
    {
        [Required]
        public required Guid MessageId { get; init; }

        [Required]
        public required Guid ChatId { get; init; }

        [Required]
        public required bool isPinned { get; init; }
    }
}
