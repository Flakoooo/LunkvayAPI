using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class UpdateChatMemberRequest
    {
        public Guid ChatId { get; init; }
        public Guid MemberId { get; init; }
        public string? NewMemberName { get; init; }
        public ChatMemberRole? NewRole { get; init; }
    }
}
