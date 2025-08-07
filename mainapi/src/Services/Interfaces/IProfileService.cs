using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Utils;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ServiceResult<UserProfileDTO>> GetUserProfileById(Guid userId);
    }
}
