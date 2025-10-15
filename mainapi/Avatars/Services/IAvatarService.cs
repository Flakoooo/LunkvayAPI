using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Avatars.Services
{
    public interface IAvatarService
    {
        Task<ServiceResult<byte[]>> GetUserAvatarByUserId(Guid userId);
    }
}
