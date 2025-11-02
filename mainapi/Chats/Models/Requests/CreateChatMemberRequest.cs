using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class CreateChatMemberRequest
    {
        [Required]
        public required Guid ChatId { get; set; }

        [Required]
        public required Guid MemberId { get; set; }

        [Required]
        public required Guid InviterId { get; set; }
    }
}
