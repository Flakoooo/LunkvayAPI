using LunkvayAPI.src.Models.Enums.ChatEnum;

namespace LunkvayChatService.Models.DTO
{
    public class ChatMemberDTO
    {
        public Guid? Id { get; set; }
        public UserDTO? Member { get; set; }
        public string? MemberName { get; set; }
        public ChatMemberRole? Role { get; set; }
    }
}
