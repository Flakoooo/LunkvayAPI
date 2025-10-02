using LunkvayAPI.src.Models.Enums.ChatEnum;

namespace LunkvayChatService.Models.DTO
{
    public record class ChatMessageDTO
    {
        public Guid? Id { get; set; }
        public UserDTO? Sender { get; set; }
        public SystemMessageType? SystemMessageType { get; set; }
        public string? Message { get; set; }
        public bool? IsEdited { get; set; }
        public bool? IsPinned { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsMyMessage { get; set; }
    }
}
