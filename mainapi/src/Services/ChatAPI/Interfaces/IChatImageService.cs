using LunkvayAPI.src.Models.Utils;

namespace LunkvayAPI.src.Services.ChatAPI.Interfaces
{
    public interface IChatImageService
    {
        Task<ServiceResult<byte[]>> GetChatImagesById(Guid chatId);
    }
}
