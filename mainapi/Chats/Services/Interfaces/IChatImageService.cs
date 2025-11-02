using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatImageService
    {
        Task<ServiceResult<string>> GetChatImgDBImage(Guid chatId);
        Task<ServiceResult<string>> UploadChatImgDBImage(Guid userId, Guid chatId, byte[] avatarData);
        Task<ServiceResult<string>> DeleteChatImgDBImage(Guid userId, Guid chatId);
    }
}
