using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Models.DTO
{
    public record class ChatDTO
    {
        public required Guid Id { get; set; }
        public string? Name { get; set; }
        public ChatMessageDTO? LastMessage { get; set; }
        public required ChatType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
    }
}
