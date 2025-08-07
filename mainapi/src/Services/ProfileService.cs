using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.Interfaces;
using LunkvayAPI.src.Utils;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.src.Services
{
    public class ProfileService(
        LunkvayDBContext lunkvayDBContext, 
        IUserService userService,
        IFriendsService friendsService
        ) : IProfileService
    {
        private readonly LunkvayDBContext _dBContext = lunkvayDBContext;
        private readonly IUserService _userService = userService;
        private readonly IFriendsService _friendsService = friendsService;

        public async Task<ServiceResult<UserProfileDTO>> GetUserProfileById(Guid userId)
        {
            UserProfile? profile = await _dBContext.Profiles.Where(up => up.UserId == userId).FirstOrDefaultAsync();
            if (profile is null)
            {
                return ServiceResult<UserProfileDTO>.Failure("Профиль не найден");
            }

            ServiceResult<UserDTO> user = await _userService.GetUserById(userId);
            if (!user.IsSuccess || user.Result is null)
            {
                return ServiceResult<UserProfileDTO>.Failure("Найден профиль без пользователя");
            }

            ServiceResult<(IEnumerable<UserListItemDTO> Friends, int FriendsCount)> result 
                = await _friendsService.GetRandomUserFriends(userId);

            UserProfileDTO profileDTO = profileDTO = UserProfileDTO.Create(
                profile.Id.ToString(), user.Result, profile.Status, profile.About,
                result.Result.FriendsCount, result.Result.Friends
            );

            return ServiceResult<UserProfileDTO>.Success(profileDTO);
        }
    }
}
