using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatMemberSystemService
    {
        Task<bool> ExistChatMembers(Guid chatId, Guid userId);
        Task<bool> ExistChatMembersOwnerOrAdministrator(Guid chatId, Guid userId);


        Task<ChatMember?> GetChatMemberByChatIdAndMemberId(Guid chatId, Guid memberId);
        Task<List<ChatMember>> GetChatMembersByChatId(Guid chatId);
        Task<ServiceResult<ChatMember>> CreateMember(
            Guid chatId, Guid memberId, ChatMemberRole role
        );
        Task<ServiceResult<List<ChatMember>>> CreatePersonalMembers(
            Guid chatId, Guid memberId1, Guid memberId2
        );
        Task<ServiceResult<List<ChatMember>>> CreateGroupMembers(
            Guid chatId, Guid creatorId, IList<Guid> members
        );
    }
}
