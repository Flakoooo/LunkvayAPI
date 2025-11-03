using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatImageService
    {
        Task<ServiceResult<byte[]>> GetChatImageByChatId(Guid chatId);
        Task<ServiceResult<byte[]>> SetChatImage(Guid userId, Guid chatId, byte[] imageData);
        Task<ServiceResult<bool>> RemoveChatImage(Guid userId, Guid chatId);
    }
}
