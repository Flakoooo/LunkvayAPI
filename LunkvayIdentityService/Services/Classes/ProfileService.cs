using LunkvayIdentityService.Models.DTO;
using LunkvayIdentityService.Models.Entities;
using LunkvayIdentityService.Models.Utils;
using LunkvayIdentityService.Services.Interfaces;
using LunkvayIdentityService.Utils;
using Microsoft.EntityFrameworkCore;

namespace LunkvayIdentityService.Services.Classes
{
    public class ProfileService(
        LunkvayDBContext lunkvayDBContext, 
        IUserService userService
    ) : IProfileService
    {
        private readonly LunkvayDBContext _dBContext = lunkvayDBContext;
        private readonly IUserService _userService = userService;

        public async Task<ServiceResult<UserProfileDTO>> GetUserProfileById(Guid userId)
        {
            UserProfile? profile = await _dBContext.Profiles.FirstOrDefaultAsync(up => up.UserId == userId);
            if (profile is null)
                return ServiceResult<UserProfileDTO>.Failure("Профиль не найден");

            ServiceResult<UserDTO> user = await _userService.GetUserById(userId);
            if (!user.IsSuccess || user.Result is null)
                return ServiceResult<UserProfileDTO>.Failure("Найден профиль без пользователя");

            UserProfileDTO profileDTO = new()
            { 
                Id = profile.Id,
                User = user.Result,
                Status = profile.Status,
                About = profile.About
            };

            return ServiceResult<UserProfileDTO>.Success(profileDTO);
        }
    }
}
