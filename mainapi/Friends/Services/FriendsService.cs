using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Friends.Models.DTO;
using LunkvayAPI.Friends.Models.Enums;
using LunkvayAPI.Users.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LunkvayAPI.Friends.Services
{
    public class FriendsService(
        ILogger<FriendsService> logger, 
        LunkvayDBContext lunkvayDBContext,
        IUserService userService
    ) : IFriendsService
    {
        private readonly ILogger<FriendsService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly IUserService _userService = userService;

        private async Task<ServiceResult<FriendDTO>> GetFriendDTO(Friendship friendship, Guid currentUserId)
        {
            var friendUserId = friendship.UserId1 == currentUserId ? friendship.UserId2 : friendship.UserId1;

            var userResult = await _userService.GetUserById(friendUserId);
            if (!userResult.IsSuccess || userResult.Result is null)
                return ServiceResult<FriendDTO>.Failure(
                    FriendsErrorCode.FriendDataRetrievalFailed.GetDescription(),
                    HttpStatusCode.InternalServerError
                );

            UserDTO user = userResult.Result;

            var labels = await _dbContext.FriendshipLabels
                .AsNoTracking()
                .Where(fl => fl.FriendshipId == friendship.Id)
                .Select(fl => fl.Label)
                .ToListAsync();

            var friend = new FriendDTO()
            {
                FriendshipId = friendship.Id,
                Status = friendship.Status,
                Labels = labels,
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsOnline = user.IsOnline
            };

            return ServiceResult<FriendDTO>.Success(friend);
        }

        private static string? StatusValidation(Guid userId, Friendship friendship, FriendshipStatus newStatus)
        {
            if (friendship.Status == newStatus)
                return FriendsErrorCode.StatusUnchanged.GetDescription();

            var isInitiator = friendship.InitiatorId == userId;

            if (isInitiator && newStatus != FriendshipStatus.Cancelled)
                return FriendsErrorCode.InitiatorCanOnlyCancel.GetDescription();

            if (!isInitiator && newStatus != FriendshipStatus.Accepted && newStatus != FriendshipStatus.Rejected)
                return FriendsErrorCode.ReceiverCanOnlyAcceptOrReject.GetDescription();

            if (friendship.Status != FriendshipStatus.Pending)
                return FriendsErrorCode.CanOnlyUpdatePendingRequests.GetDescription();

            return null;
        }

        public async Task<ServiceResult<List<FriendDTO>>> GetFriends(
            Guid userId,
            int page,
            int pageSize,
            bool isCurrentUser = false
        )
        {
            _logger.LogInformation(
                "({Date}) Запрос друзей пользователя {Id} (страница {Page}, размер {PageSize})",
                DateTime.UtcNow, userId, page, pageSize
            );

            if (userId == Guid.Empty)
                return ServiceResult<List<FriendDTO>>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var friendships = await _dbContext.Friendships
                .AsNoTracking()
                .Where(f => f.Status == FriendshipStatus.Accepted && (f.UserId1 == userId || f.UserId2 == userId))
                .Select(f => new { f.Id, FriendId = f.UserId1 == userId ? f.UserId2 : f.UserId1, f.Status })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Dictionary<Guid, List<string?>>? labelsMap = null;
            if (isCurrentUser && friendships.Count != 0)
            {
                var friendshipIds = friendships.Select(f => f.Id).ToList();
                labelsMap = await _dbContext.FriendshipLabels
                    .AsNoTracking()
                    .Where(fl => friendshipIds.Contains(fl.FriendshipId))
                    .GroupBy(fl => fl.FriendshipId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(fl => fl.Label).ToList()
                    );
            }

            var friends = new List<FriendDTO>();
            foreach (var f in friendships)
            {
                ServiceResult<UserDTO> userResult = await _userService.GetUserById(f.FriendId);
                if (!userResult.IsSuccess || userResult.Result is null)
                    return ServiceResult<List<FriendDTO>>.Failure(
                        userResult.Error ?? ErrorCode.InternalServerError.GetDescription(),
                        HttpStatusCode.InternalServerError
                    );

                UserDTO user = userResult.Result;

                var friend = new FriendDTO
                {
                    FriendshipId = f.Id,
                    Status = isCurrentUser ? f.Status : null,
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsOnline = user.IsOnline,
                    Labels = isCurrentUser ? (labelsMap?.GetValueOrDefault(f.Id) ?? []) : null
                };
                friends.Add(friend);
            }

            _logger.LogInformation("({Date}) Получено {Count} друзей", DateTime.UtcNow, friendships.Count);
            return ServiceResult<List<FriendDTO>>.Success(friends);
        }

        public async Task<ServiceResult<RandomFriendsResult>> GetRandomFriends(Guid userId, int count)
        {
            _logger.LogInformation("({Date}) Запрос друзей пользователя {Id} для профиля", DateTime.UtcNow, userId);

            if (userId == Guid.Empty)
                return ServiceResult<RandomFriendsResult>.Failure(ErrorCode.UserIdRequired.GetDescription());

            List<Guid> friendIds = await _dbContext.Friendships
                .AsNoTracking()
                .Where(f => f.Status == FriendshipStatus.Accepted && (f.UserId1 == userId || f.UserId2 == userId))
                .Select(f => f.UserId1 == userId ? f.UserId2 : f.UserId1)
                .ToListAsync();

            List<UserListItemDTO> friends = [];

            if (friendIds.Count >= count)
            {
                Random random = new();
                friendIds = [.. friendIds.OrderBy(x => random.Next()).Take(count)];
            }

            friends = await _dbContext.Users
                .AsNoTracking()
                .Where(u => friendIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => new UserListItemDTO
                {
                    UserId = u.Id,
                    FirstName = u.FirstName,
                    IsOnline = u.IsOnline,
                    LastName = u.LastName
                })
                .ToListAsync();

            _logger.LogInformation(
                "({Date}) Получено {Count} друзей, всего {CountAll} друзей", 
                DateTime.UtcNow, friends.Count, friendIds.Count
            );

            return ServiceResult<RandomFriendsResult>.Success(
                new() { Friends = friends, FriendsCount = friends.Count }
            );
        }

        public async Task<ServiceResult<FriendDTO>> CreateFriendShip(Guid initiatorId, Guid friendId)
        {
            if (initiatorId == Guid.Empty || friendId == Guid.Empty)
                return ServiceResult<FriendDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (initiatorId == friendId)
                return ServiceResult<FriendDTO>.Failure(FriendsErrorCode.CannotAddSelfAsFriend.GetDescription());

            var userId1 = initiatorId.CompareTo(friendId) < 0 ? initiatorId : friendId;
            var userId2 = initiatorId.CompareTo(friendId) < 0 ? friendId : initiatorId;

            var friendship = await _dbContext.Friendships
                .FirstOrDefaultAsync(f => f.UserId1 == userId1 && f.UserId2 == userId2);

            if (friendship != null)
            {
                if (friendship.Status == FriendshipStatus.Pending)
                    return ServiceResult<FriendDTO>.Failure(FriendsErrorCode.FriendRequestAlreadyExists.GetDescription());

                if (friendship.Status == FriendshipStatus.Accepted)
                    return ServiceResult<FriendDTO>.Failure(FriendsErrorCode.AlreadyFriends.GetDescription());

                if (friendship.Status == FriendshipStatus.Cancelled || friendship.Status == FriendshipStatus.Rejected)
                {
                    friendship.Status = FriendshipStatus.Pending;
                    friendship.InitiatorId = initiatorId;
                    friendship.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                friendship = new Friendship()
                {
                    UserId1 = userId1,
                    UserId2 = userId2,
                    Status = FriendshipStatus.Pending,
                    InitiatorId = initiatorId
                };
                await _dbContext.Friendships.AddAsync(friendship);
            }

            await _dbContext.SaveChangesAsync();

            return await GetFriendDTO(friendship, initiatorId);
        }

        public async Task<ServiceResult<FriendDTO>> UpdateFriendShipStatus(Guid userId, Guid friendshipId, FriendshipStatus status)
        {
            if (userId == Guid.Empty)
                return ServiceResult<FriendDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (friendshipId == Guid.Empty)
                return ServiceResult<FriendDTO>.Failure(FriendsErrorCode.FriendshipIdRequired.GetDescription());

            var friendship = await _dbContext.Friendships
                .FirstOrDefaultAsync(f => f.Id == friendshipId &&
                    (f.UserId1 == userId || f.UserId2 == userId)
                );

            if (friendship is null)
                return ServiceResult<FriendDTO>.Failure(
                    FriendsErrorCode.FriendshipNotFound.GetDescription(), HttpStatusCode.NotFound
                );

            string? statusError = StatusValidation(userId, friendship, status);
            if (statusError != null)
                return ServiceResult<FriendDTO>.Failure(statusError);

            friendship.Status = status;
            friendship.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return await GetFriendDTO(friendship, userId);
        }
    }
}
