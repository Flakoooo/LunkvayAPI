using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Friends.Services;
using LunkvayAPI.Profiles.Models.DTO;
using LunkvayAPI.Profiles.Models.Enums;
using LunkvayAPI.Profiles.Models.Requests;
using LunkvayAPI.Users.Services;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.Profiles.Services
{
    public class ProfileService(
        LunkvayDBContext lunkvayDBContext, 
        IUserService userService,
        IFriendshipsService friendshipsService
    ) : IProfileService
    {
        private readonly LunkvayDBContext _dBContext = lunkvayDBContext;
        private readonly IUserService _userService = userService;
        private readonly IFriendshipsService _friendshipsService = friendshipsService;

        public async Task<ServiceResult<ProfileDTO>> GetUserProfileById(Guid userId)
        {
            if (userId == Guid.Empty)
                return ServiceResult<ProfileDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var profile = await _dBContext.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile is null)
                return ServiceResult<ProfileDTO>.Failure(ProfilesErrorCode.ProfileNotFound.GetDescription());

            ServiceResult<UserDTO> userResult = await _userService.GetUserById(userId);
            if (!userResult.IsSuccess || userResult.Result is null)
                return ServiceResult<ProfileDTO>.Failure(
                    ProfilesErrorCode.ProfileWithoutUser.GetDescription(), userResult.StatusCode
                );

            ServiceResult<RandomFriendsResult> friendsResult 
                = await _friendshipsService.GetRandomFriends(userId);

            var profileDTO = new ProfileDTO()
            { 
                Id = profile.Id,
                User = userResult.Result,
                Status = profile.Status,
                About = profile.About,
                FriendsCount = friendsResult?.Result?.FriendsCount,
                Friends = friendsResult?.Result?.Friends
            };

            return ServiceResult<ProfileDTO>.Success(profileDTO);
        }

        public async Task<ServiceResult<Profile>> CreateProfile(Guid userId)
        {
            if (userId == Guid.Empty)
                return ServiceResult<Profile>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var profile = new Profile() { UserId = userId };

            await _dBContext.Profiles.AddAsync(profile);
            await _dBContext.SaveChangesAsync();

            return ServiceResult<Profile>.Success(profile);
        }

        public async Task<ServiceResult<ProfileDTO>> UpdateProfile(Guid userId, UpdateProfileRequest request)
        {
            if (userId == Guid.Empty)
                return ServiceResult<ProfileDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var profile = await _dBContext.Profiles.FirstOrDefaultAsync(up => up.UserId == userId);
            if (profile is null)
                return ServiceResult<ProfileDTO>.Failure(ProfilesErrorCode.ProfileNotFound.GetDescription());

            var hasChanges = false;

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
