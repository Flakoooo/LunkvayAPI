using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Models.DTO
{
    public class ChatMemberDTO
    {
        public required Guid Id { get; set; }
        public required Guid UserId { get; set; }
        public required string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsOnline { get; set; } = false;
        public string? MemberName { get; set; }
        public required ChatMemberRole Role { get; set; }
    }
}
