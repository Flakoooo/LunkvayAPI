using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatImageService
    {
        Task<ServiceResult<byte[]>> GetChatImagesById(Guid chatId);
    }
}
