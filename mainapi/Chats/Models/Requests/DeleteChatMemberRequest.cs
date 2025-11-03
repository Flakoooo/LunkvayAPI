using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class DeleteChatMemberRequest
    {
        [Required]
        public required Guid ChatId { get; init; }

        [Required]
        public required Guid MemberId { get; init; }
    }
}
