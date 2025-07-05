using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Utils;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IProfileService
    {
        public Task<UserProfileDTO> GetUserProfileById(Guid userId);
    }
}
