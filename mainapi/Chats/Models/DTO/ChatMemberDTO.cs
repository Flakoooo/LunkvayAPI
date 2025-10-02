using LunkvayAPI.Common.DTO;
using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Models.DTO
{
    public class ChatMemberDTO
    {
        public Guid? Id { get; set; }
        public UserDTO? Member { get; set; }
        public string? MemberName { get; set; }
        public ChatMemberRole? Role { get; set; }
    }
}
