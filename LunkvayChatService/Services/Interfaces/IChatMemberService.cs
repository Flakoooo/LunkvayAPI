using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;
using LunkvayChatService.Models.DTO;

namespace LunkvayAPI.src.Services.ChatAPI.Interfaces
{
    public interface IChatMemberService
    {
        Task<ServiceResult<IEnumerable<ChatMemberDTO>>> GetChatMembers(Guid chatId);
        Task<ServiceResult<ChatMemberDTO>> CreateMember(ChatMemberRequest chatMemberRequest);
    }
}
