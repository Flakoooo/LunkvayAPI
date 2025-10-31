using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class ChatMemberRequest
    {
        [Required]
        public Guid ChatId { get; set; }

        [Required]
        public Guid MemberId { get; set; }

        [Required]
        public Guid InviterId { get; set; }
    }
}
