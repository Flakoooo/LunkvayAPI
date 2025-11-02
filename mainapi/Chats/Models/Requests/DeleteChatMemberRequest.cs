using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class DeleteChatMemberRequest
    {
        [Required]
        public required Guid ChatId { get; set; }

        [Required]
        public required Guid MemberId { get; set; }
    }
}
