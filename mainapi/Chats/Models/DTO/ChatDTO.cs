namespace LunkvayAPI.Chats.Models.DTO
{
    public record class ChatDTO
    {
        public required Guid Id { get; set; }
        public ChatMessageDTO? LastMessage { get; set; }
        public string? Name { get; set; }
    }
}
