using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatSystemService
    {
        Task<ServiceResult<Chat>> GetChat(Guid chatId);
        Task<Guid?> FindPersonalChatBetweenUsers(
            Guid user1Id, Guid user2Id
        );
        Task<ServiceResult<Chat>> CreatePersonalChat(
            ChatType chatType, string? name
        );
        Task<ServiceResult<Chat>> UpdateChatLastMessage(
            Guid chatId, Guid lastMessageId
        );
    }
}
