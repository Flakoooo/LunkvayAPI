using LunkvayAPI.Common.Results;
using LunkvayAPI.Profiles.Models.DTO;
using LunkvayAPI.Profiles.Models.Requests;

namespace LunkvayAPI.Profiles.Services
{
    public interface IProfileService
    {
        Task<ServiceResult<ProfileDTO>> GetUserProfileById(Guid userId);
        Task<ServiceResult<ProfileDTO>> UpdateProfile(Guid userId, UpdateProfileRequest request);
    }
}
