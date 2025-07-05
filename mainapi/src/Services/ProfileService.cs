using LunkvayAPI.src.Models.DTO;
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

        public async Task<UserProfileDTO> GetUserProfileById(Guid userId)
        {
            var profile = await _dBContext.Profiles.Where(up => up.UserId == userId).FirstOrDefaultAsync() 
                ?? throw new Exception("Профиль не найден");

            var user = await _userService.GetUserById(userId)
                ?? throw new Exception("Найден профиль без пользователя");

            var (randomFriends, friendsCount) = await _friendsService.GetRandomUserFriends(userId);

            var profileDTO = new UserProfileDTO
            {
                Id = profile.Id.ToString(),
                User = user,
                Status = profile.Status,
                About = profile.About,
                FriendsCount = friendsCount,
                Friends = randomFriends
            };

            return profileDTO;
        }
    }
}
