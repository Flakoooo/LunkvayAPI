using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatMessageSystemService
    {
        Task<ServiceResult<ChatMessage>> CreateSystemChatMessage(
            Guid chatId, string message, SystemMessageType type
        );
    }
}
