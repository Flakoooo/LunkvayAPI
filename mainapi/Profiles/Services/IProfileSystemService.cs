using LunkvayAPI.Data.Entities;

namespace LunkvayAPI.Profiles.Services
{
    public interface IProfileSystemService
    {
        Task<Profile?> CreateProfile(Guid userId);
        Task<Profile?> UpdateProfileUpdatedTime(Guid userId);
    }
}
