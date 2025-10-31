using LunkvayAPI.Common.DTO;
using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Models.DTO
{
    public record class ChatMessageDTO
    {
        public required Guid Id { get; set; }
        public UserDTO? Sender { get; set; }
        public required SystemMessageType SystemMessageType { get; set; }
        public required string Message { get; set; }
        public bool? IsEdited { get; set; }
        public bool? IsPinned { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsMyMessage { get; set; }
    }
}
