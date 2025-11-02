using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatSystemService
    {
        Task<ServiceResult<Chat>> GetChatBySystem(Guid chatId);
        Task<ServiceResult<Chat>> UpdateChatLastMessageBySystem(
            Guid chatId, Guid lastMessageId
        );
    }
}
