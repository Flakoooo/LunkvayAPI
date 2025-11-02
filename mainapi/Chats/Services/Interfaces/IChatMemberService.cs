using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using System.Linq.Expressions;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatMemberService
    {
        Task<bool> ExistAnyChatMembersBySystem(Expression<Func<ChatMember, bool>> predicate);
        Task<ServiceResult<ChatMember>> CreateMemberBySystem(
            Guid chatId, Guid memberId, ChatMemberRole role
        );
        Task<ServiceResult<List<ChatMember>>> CreatePersonalMembersBySystem(
            Guid chatId, Guid memberId1, Guid memberId2
        );
        Task<ServiceResult<List<ChatMember>>> CreateGroupMembersBySystem(
            Guid chatId, Guid creatorId, IList<UserDTO> members
        );


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
