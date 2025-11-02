using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatMemberService
    {
        Task<ServiceResult<List<ChatMemberDTO>>> GetChatMembers(Guid chatId);
        Task<ServiceResult<ChatMemberDTO>> CreateMember(
            Guid initiatorId, CreateChatMemberRequest request
        );
        Task<ServiceResult<ChatMemberDTO>> UpdateMember(
            Guid initiatorId, UpdateChatMemberRequest request
        );
        Task<ServiceResult<bool>> DeleteMember(
            Guid initiatorId, DeleteChatMemberRequest request
        );
    }
}
