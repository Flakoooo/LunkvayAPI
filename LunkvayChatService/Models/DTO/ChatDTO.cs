namespace LunkvayChatService.Models.DTO
{
    public record class ChatDTO
    {
        public Guid? Id { get; set; }
        public ChatMessageDTO? LastMessage { get; set; }
        public string? Name { get; set; }
    }
}
