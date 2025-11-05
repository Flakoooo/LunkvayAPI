using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Models.DTO
{
    public record class ChatMessageDTO
    {
        public Guid Id { get; set; }
        public Guid? SenderId { get; set; } = Guid.Empty;
        public string? SenderUserName { get; set; }
        public string? SenderFirstName { get; set; }
        public string? SenderLastName { get; set; }
        public bool? SenderIsOnline { get; set; }
        public required SystemMessageType SystemMessageType { get; set; }
        public required string Message { get; set; }
        public bool IsEdited { get; set; } = false;
        public bool IsPinned { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsMyMessage { get; set; } = false;
    }
}
