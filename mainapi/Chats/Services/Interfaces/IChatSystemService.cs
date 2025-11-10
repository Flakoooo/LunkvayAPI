using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatSystemService
    {
        Task<ServiceResult<Chat>> GetChatBySystem(Guid chatId);
        Task<Guid?> FindPersonalChatBetweenUsersBySystem(
            Guid user1Id, Guid user2Id
        );
        Task<ServiceResult<Chat>> CreatePersonalChatBySystem(
            ChatType chatType, string? name
        );
        Task<ServiceResult<Chat>> UpdateChatLastMessageBySystem(
            Guid chatId, Guid lastMessageId
        );
    }
}
