using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Profiles.Models.DTO;
using LunkvayAPI.Profiles.Models.Requests;

namespace LunkvayAPI.Profiles.Services
{
    public interface IProfileService
    {
        Task<ServiceResult<ProfileDTO>> GetUserProfileById(Guid userId);
        Task<ServiceResult<Profile>> CreateProfile(Guid userId);
        Task<ServiceResult<ProfileDTO>> UpdateProfile(Guid userId, UpdateProfileRequest request);
    }
}
