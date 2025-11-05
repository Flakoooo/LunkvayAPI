using LunkvayAPI.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class UpdateChatMemberRequest
    {
        [Required]
        public required Guid ChatId { get; init; }

        [Required]
        public required Guid MemberId { get; init; }


        public string? NewMemberName { get; init; }
        public ChatMemberRole? NewRole { get; init; }
    }
}
