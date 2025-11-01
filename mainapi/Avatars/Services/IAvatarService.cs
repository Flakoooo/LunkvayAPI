using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Avatars.Services
{
    public interface IAvatarService
    {
        Task<ServiceResult<string>> GetUserImgDBAvatar(Guid userId);
        Task<ServiceResult<string>> UploadUserImgDBAvatar(Guid userId, byte[] avatarData);
        Task<ServiceResult<string>> DeleteUserImgDBAvatar(Guid userId);
    }
}
