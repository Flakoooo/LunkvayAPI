using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Avatars.Services
{
    public interface IAvatarService
    {
        Task<ServiceResult<byte[]>> GetUserAvatarByUserId(Guid userId);
        Task<ServiceResult<byte[]>> SetUserAvatar(Guid userId, byte[] avatarData);
        Task<ServiceResult<bool>> RemoveUserAvatar(Guid userId);
    }
}
