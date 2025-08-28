using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;

namespace LunkvayAPI.src.Services.ChatAPI.Interfaces
{
    public interface IChatMemberService
    {
        Task<ServiceResult<IEnumerable<ChatMemberDTO>>> GetChatMembers(Guid chatId);
        Task<ServiceResult<ChatMemberDTO>> CreateMember(ChatMemberRequest chatMemberRequest);
    }
}
