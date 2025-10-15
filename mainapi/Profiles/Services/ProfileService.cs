using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Friends.Services;
using LunkvayAPI.Profiles.Models.DTO;
using LunkvayAPI.Profiles.Models.Requests;
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
            if (userId == Guid.Empty)
                return ServiceResult<ProfileDTO>.Failure(ErrorCode.UserIdIsNull.GetDescription());

            Profile? profile = await _dBContext.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (profile is null)
                return ServiceResult<ProfileDTO>.Failure(ErrorCode.ProfileNotFound.GetDescription());

            ServiceResult<UserDTO> user = await _userService.GetUserById(userId);
            if (!user.IsSuccess || user.Result is null)
                return ServiceResult<ProfileDTO>.Failure(ErrorCode.ProfileWithoutUser.GetDescription());

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

        public async Task<ServiceResult<Profile>> CreateProfile(Guid userId)
        {
            if (userId == Guid.Empty)
                return ServiceResult<Profile>.Failure(ErrorCode.UserIdIsNull.GetDescription());

            Profile profile = new() { UserId = userId };

            await _dBContext.Profiles.AddAsync(profile);
            await _dBContext.SaveChangesAsync();

            return ServiceResult<Profile>.Success(profile);
        }

        public async Task<ServiceResult<ProfileDTO>> UpdateProfile(Guid userId, UpdateProfileRequest request)
        {
            if (userId == Guid.Empty)
                return ServiceResult<ProfileDTO>.Failure(ErrorCode.UserIdIsNull.GetDescription());

            Profile? profile = await _dBContext.Profiles.FirstOrDefaultAsync(up => up.UserId == userId);
            if (profile is null)
                return ServiceResult<ProfileDTO>.Failure(ErrorCode.ProfileNotFound.GetDescription());

            bool hasChanges = false;

            if (!string.IsNullOrEmpty(request.NewAbout))
            {
                profile.About = request.NewAbout;
                hasChanges = true;
            }
            if (!string.IsNullOrEmpty(request.NewStatus))
            {
                profile.Status = request.NewStatus;
                hasChanges = true;
            }

            if (hasChanges) await _dBContext.SaveChangesAsync();

            return await GetUserProfileById(userId);
        }
    }
}
