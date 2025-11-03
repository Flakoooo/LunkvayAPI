using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class CreateChatMemberRequest
    {
        [Required]
        public required Guid ChatId { get; init; }

        [Required]
        public required Guid MemberId { get; init; }

        [Required]
        public required Guid InviterId { get; init; }
    }
}
