using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Enums.ErrorCodes;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data;
using LunkvayAPI.Friends.Services;
using LunkvayAPI.Profiles.Models.DTO;
using LunkvayAPI.Profiles.Models.Enums;
using LunkvayAPI.Profiles.Models.Requests;
using LunkvayAPI.Users.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LunkvayAPI.Profiles.Services
{
    public class ProfileService(
        LunkvayDBContext lunkvayDBContext, 
        IUserSystemService userService,
        IFriendshipsSystemService friendshipsService
    ) : IProfileService
    {
        private readonly LunkvayDBContext _dBContext = lunkvayDBContext;
        private readonly IUserSystemService _userService = userService;
        private readonly IFriendshipsSystemService _friendshipsService = friendshipsService;

        public async Task<ServiceResult<ProfileDTO>> GetUserProfileById(Guid userId)
        {
            if (userId == Guid.Empty)
                return ServiceResult<ProfileDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var profile = await _dBContext.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile is null)
                return ServiceResult<ProfileDTO>.Failure(ProfilesErrorCode.ProfileNotFound.GetDescription());

            var user = await _userService.GetUserById(userId);
            if (user is null)
                return ServiceResult<ProfileDTO>.Failure(
                    UsersErrorCode.UserNotFound.GetDescription(), 
                    HttpStatusCode.NotFound
                );

            RandomFriendsResult? friendsResult 
                = await _friendshipsService.GetRandomFriends(userId);

            var profileDTO = new ProfileDTO()
            { 
                Id = profile.Id,
                User = new UserDTO
                { 
                    Id = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsDeleted = user.IsDeleted,
                    IsOnline = user.IsOnline,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    LastLogin = user.LastLogin
                },
                Status = profile.Status,
                About = profile.About,
                FriendsCount = friendsResult?.FriendsCount,
                Friends = friendsResult?.Friends,
                UpdatedAt = profile.UpdatedAt
            };

            return ServiceResult<ProfileDTO>.Success(profileDTO);
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

            if (hasChanges)
            {
                profile.UpdatedAt = DateTime.UtcNow;
                await _dBContext.SaveChangesAsync();
            }

            return await GetUserProfileById(userId);
        }
    }
}
