using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using System.Linq.Expressions;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatMemberSystemService
    {
        Task<bool> ExistAnyChatMembersBySystem(Expression<Func<ChatMember, bool>> predicate);
        Task<List<ChatMember>> GetChatMembersByChatIdBySystem(Guid chatId);
        Task<ServiceResult<ChatMember>> CreateMemberBySystem(
            Guid chatId, Guid memberId, ChatMemberRole role
        );
        Task<ServiceResult<List<ChatMember>>> CreatePersonalMembersBySystem(
            Guid chatId, Guid memberId1, Guid memberId2
        );
        Task<ServiceResult<List<ChatMember>>> CreateGroupMembersBySystem(
            Guid chatId, Guid creatorId, IList<Guid> members
        );
    }
}
