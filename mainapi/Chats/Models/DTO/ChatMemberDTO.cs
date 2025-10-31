using LunkvayAPI.Common.DTO;
using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Models.DTO
{
    public class ChatMemberDTO
    {
        public required Guid Id { get; set; }
        public required UserDTO Member { get; set; }
        public string? MemberName { get; set; }
        public required ChatMemberRole Role { get; set; }
    }
}
