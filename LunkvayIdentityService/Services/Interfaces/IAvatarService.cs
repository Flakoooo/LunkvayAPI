using LunkvayIdentityService.Models.Utils;

namespace LunkvayIdentityService.Services.Interfaces
{
    public interface IAvatarService
    {
        Task<ServiceResult<byte[]>> GetUserAvatarById(Guid userId);
    }
}
