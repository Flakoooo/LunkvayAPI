using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class UpdateChatMemberRequest
    {
        public Guid ChatId { get; set; }
        public Guid MemberId { get; set; }
        public string? NewMemberName { get; set; }
        public ChatMemberRole? NewRole { get; set; }
    }
}
