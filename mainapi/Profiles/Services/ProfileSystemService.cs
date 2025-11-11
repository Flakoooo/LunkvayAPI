using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.Profiles.Services
{
    public class ProfileSystemService(LunkvayDBContext lunkvayDBContext) : IProfileSystemService
    {
        private readonly LunkvayDBContext _dBContext = lunkvayDBContext;

        public async Task<Profile?> CreateProfile(Guid userId)
        {
            if (userId == Guid.Empty) return null;

            var profile = new Profile() { UserId = userId };

            await _dBContext.Profiles.AddAsync(profile);
            await _dBContext.SaveChangesAsync();

            return profile;
        }

        public async Task<Profile?> UpdateProfileUpdatedTime(Guid userId)
        {
            if (userId == Guid.Empty) return null;

            var profile = await _dBContext.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile is null) return null;

            profile.UpdatedAt = DateTime.UtcNow;

            await _dBContext.SaveChangesAsync();

            return profile;
        }
    }
}
