using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatMemberService
    {
        Task<ServiceResult<IEnumerable<ChatMemberDTO>>> GetChatMembers(Guid chatId);
        Task<ServiceResult<ChatMemberDTO>> CreateMember(ChatMemberRequest chatMemberRequest);
    }
}
