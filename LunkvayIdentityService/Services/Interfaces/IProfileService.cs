using LunkvayIdentityService.Models.DTO;
using LunkvayIdentityService.Models.Utils;

namespace LunkvayIdentityService.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ServiceResult<UserProfileDTO>> GetUserProfileById(Guid userId);
    }
}
