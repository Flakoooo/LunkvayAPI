using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Friends.Services;
using LunkvayAPI.Profiles.Models.DTO;
using LunkvayAPI.Users.Services;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.Profiles.Services
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

        public async Task<ServiceResult<ProfileDTO>> GetUserProfileById(Guid userId)
        {
            Profile? profile = await _dBContext.Profiles.FirstOrDefaultAsync(up => up.UserId == userId);
            if (profile is null)
                return ServiceResult<ProfileDTO>.Failure("Профиль не найден");

            ServiceResult<UserDTO> user = await _userService.GetUserById(userId);
            if (!user.IsSuccess || user.Result is null)
                return ServiceResult<ProfileDTO>.Failure("Найден профиль без пользователя");

            ServiceResult<(IEnumerable<UserListItemDTO> Friends, int FriendsCount)> result 
                = await _friendsService.GetRandomUserFriends(userId);

            ProfileDTO profileDTO = new()
            { 
                Id = profile.Id,
                User = user.Result,
                Status = profile.Status,
                About = profile.About,
                FriendsCount = result.Result.FriendsCount,
                Friends = result.Result.Friends
            };

            return ServiceResult<ProfileDTO>.Success(profileDTO);
        }
    }
}
