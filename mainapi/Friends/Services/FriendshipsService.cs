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
using System.Linq.Expressions;
using System.Net;

namespace LunkvayAPI.Friends.Services
{
    public class FriendshipsService(
        ILogger<FriendshipsService> logger,
        LunkvayDBContext lunkvayDBContext,
        IUserService userService
    ) : IFriendshipsService
    {
        private readonly ILogger<FriendshipsService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly IUserService _userService = userService;

        private async Task<ServiceResult<FriendshipDTO>> GetFriendDTO(Friendship friendship, Guid currentUserId)
        {
            var friendUserId = friendship.UserId1 == currentUserId ? friendship.UserId2 : friendship.UserId1;

            var userResult = await _userService.GetUserById(friendUserId);
            if (!userResult.IsSuccess || userResult.Result is null)
                return ServiceResult<FriendshipDTO>.Failure(
                    FriendshipErrorCode.FriendDataRetrievalFailed.GetDescription(),
                    HttpStatusCode.InternalServerError
                );

            UserDTO user = userResult.Result;

            var labels = await _dbContext.FriendshipLabels
                .AsNoTracking()
                .Where(fl => fl.FriendshipId == friendship.Id)
                .Select(fl => new FriendshipLabelDTO { Id = fl.Id, Label = fl.Label })
                .ToListAsync();

            var friend = new FriendshipDTO()
            {
                FriendshipId = friendship.Id,
                Status = friendship.Status,
                Labels = labels,
                UserId = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsOnline = user.IsOnline
            };

            return ServiceResult<FriendshipDTO>.Success(friend);
        }

        private async Task<List<FriendshipDTO>> GetFriendsDTO(
            Guid userId, int page, int pageSize,
            Expression<Func<Friendship, bool>> expression,
            bool includeStatus,
            bool includeLabels
        )
        {
            var query = _dbContext.Friendships
                .AsNoTracking()
                .Where(expression)
                .Select(f => new FriendshipDTO
                {
                    FriendshipId = f.Id,
                    Status = includeStatus ? f.Status : null,
                    UserId = f.UserId1 == userId ? f.UserId2 : f.UserId1,
                    UserName = f.UserId1 == userId ? f.User2!.UserName : f.User1!.UserName,
                    FirstName = f.UserId1 == userId ? f.User2!.FirstName : f.User1!.FirstName,
                    LastName = f.UserId1 == userId ? f.User2!.LastName : f.User1!.LastName,
                    IsOnline = f.UserId1 == userId ? f.User2!.IsOnline : f.User1!.IsOnline,
                    Labels = includeLabels ? f.Labels.Select(l => new FriendshipLabelDTO
                    {
                        Id = l.Id,
                        Label = l.Label
                    }).ToList() : null
                });

            var friends = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return friends;
        }

        private static string? StatusValidation(Guid userId, Friendship friendship, FriendshipStatus newStatus)
        {
            if (friendship.Status == newStatus)
                return FriendshipErrorCode.StatusUnchanged.GetDescription();

            var isInitiator = friendship.InitiatorId == userId;

            return friendship.Status switch
            {
                FriendshipStatus.Pending => ValidatePendingStatus(newStatus, isInitiator),
                FriendshipStatus.Accepted => ValidateAcceptedStatus(newStatus),
                _ => "Статус дружбы невозможно изменить",
            };
        }

        private static string? ValidatePendingStatus(FriendshipStatus newStatus, bool isInitiator)
        {
            if (isInitiator && newStatus != FriendshipStatus.Cancelled)
                return FriendshipErrorCode.InitiatorCanOnlyCancel.GetDescription();

            if (!isInitiator && newStatus != FriendshipStatus.Accepted && newStatus != FriendshipStatus.Rejected)
                return FriendshipErrorCode.ReceiverCanOnlyAcceptOrReject.GetDescription();

            return null;
        }

        private static string? ValidateAcceptedStatus(FriendshipStatus newStatus)
        {
            if (newStatus == FriendshipStatus.Deleted)
                return null;

            return "Текущие дружеские отношения можно только отменить";
        }

        public async Task<ServiceResult<List<FriendshipDTO>>> GetFriends(
            Guid userId, int page, int pageSize, bool isCurrentUser
        )
        {
            _logger.LogInformation(
                "({Date}) Запрос друзей пользователя {Id} (страница {Page}, размер {PageSize})",
                DateTime.UtcNow, userId, page, pageSize
            );

            if (userId == Guid.Empty)
                return ServiceResult<List<FriendshipDTO>>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var friends = await GetFriendsDTO(
                userId, page, pageSize,
                f => f.Status == FriendshipStatus.Accepted && (f.UserId1 == userId || f.UserId2 == userId),
                isCurrentUser,
                isCurrentUser
            );

            _logger.LogInformation("({Date}) Получено {Count} друзей", DateTime.UtcNow, friends.Count);
            return ServiceResult<List<FriendshipDTO>>.Success(friends);
        }

        public async Task<ServiceResult<List<FriendshipDTO>>> GetIncomingFriendRequests(
            Guid userId, int page, int pageSize
        )
        {
            _logger.LogInformation(
                "({Date}) Запрос исходящий заявок в друзья пользователя {Id} (страница {Page}, размер {PageSize})",
                DateTime.UtcNow, userId, page, pageSize
            );

            if (userId == Guid.Empty)
                return ServiceResult<List<FriendshipDTO>>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var friends = await GetFriendsDTO(
                userId, page, pageSize,
                f => f.InitiatorId != userId && 
                     f.Status == FriendshipStatus.Pending && 
                     (f.UserId1 == userId || f.UserId2 == userId),
                true,
                false
            );

            _logger.LogInformation("({Date}) Получено {Count} исходящих заявок", DateTime.UtcNow, friends.Count);
            return ServiceResult<List<FriendshipDTO>>.Success(friends);
        }

        public async Task<ServiceResult<List<FriendshipDTO>>> GetOutgoingFriendRequests(
            Guid userId, int page, int pageSize
        )
        {
            _logger.LogInformation(
                "({Date}) Запрос входящих заявок в друзья пользователя {Id} (страница {Page}, размер {PageSize})",
                DateTime.UtcNow, userId, page, pageSize
            );

            if (userId == Guid.Empty)
                return ServiceResult<List<FriendshipDTO>>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var friends = await GetFriendsDTO(
                userId, page, pageSize,
                f => f.InitiatorId == userId && f.Status == FriendshipStatus.Pending,
                true,
                false
            );

            _logger.LogInformation("({Date}) Получено {Count} входящих заявок", DateTime.UtcNow, friends.Count);
            return ServiceResult<List<FriendshipDTO>>.Success(friends);
        }

        public async Task<ServiceResult<List<UserListItemDTO>>> GetPossibleFriends(
            Guid userId, int page, int pageSize
        )
        {
            if (userId == Guid.Empty)
                return ServiceResult<List<UserListItemDTO>>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var users = await _dbContext.Users
                .Where(u => u.Id != userId)
                .Where(u => !_dbContext.Friendships
                    .Any(f => (f.UserId1 == userId && f.UserId2 == u.Id) || 
                              (f.UserId2 == userId && f.UserId1 == u.Id))
                )
                .OrderByDescending(u => _dbContext.Friendships
                    .Count(f => f.UserId1 == u.Id || f.UserId2 == u.Id)
                )
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListItemDTO
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsOnline = u.IsOnline,
                })
                .ToListAsync();

            return ServiceResult<List<UserListItemDTO>>.Success(users);
        }

        public async Task<ServiceResult<RandomFriendsResult>> GetRandomFriends(Guid userId, int count)
        {
            _logger.LogInformation("({Date}) Запрос друзей пользователя {Id} для профиля", DateTime.UtcNow, userId);

            if (userId == Guid.Empty)
                return ServiceResult<RandomFriendsResult>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var query = _dbContext.Friendships
                .AsNoTracking()
                .Where(f => f.Status == FriendshipStatus.Accepted && (f.UserId1 == userId || f.UserId2 == userId));

            var friendsCount = await query.CountAsync();

            var friends = await query
                .OrderBy(x => EF.Functions.Random())
                .Take(count)
                .Select(f => new UserListItemDTO
                {
                    UserId = f.UserId1 == userId ? f.UserId2 : f.UserId1,
                    UserName = f.UserId1 == userId ? f.User2!.UserName : f.User1!.UserName,
                    FirstName = f.UserId1 == userId ? f.User2!.FirstName : f.User1!.FirstName,
                    LastName = f.UserId1 == userId ? f.User2!.LastName : f.User1!.LastName,
                    IsOnline = f.UserId1 == userId ? f.User2!.IsOnline : f.User1!.IsOnline
                })
                .ToListAsync();

            _logger.LogInformation(
                "({Date}) Получено {Count} друзей, всего {CountAll} друзей",
                DateTime.UtcNow, friends.Count, friendsCount
            );

            return ServiceResult<RandomFriendsResult>.Success(
                new() { Friends = friends, FriendsCount = friendsCount }
            );
        }

        public async Task<ServiceResult<FriendshipDTO>> CreateFriendShip(Guid initiatorId, Guid friendId)
        {
            if (initiatorId == Guid.Empty || friendId == Guid.Empty)
                return ServiceResult<FriendshipDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (initiatorId == friendId)
                return ServiceResult<FriendshipDTO>.Failure(FriendshipErrorCode.CannotAddSelfAsFriend.GetDescription());

            var userId1 = initiatorId.CompareTo(friendId) < 0 ? initiatorId : friendId;
            var userId2 = initiatorId.CompareTo(friendId) < 0 ? friendId : initiatorId;

            var friendship = await _dbContext.Friendships
                .FirstOrDefaultAsync(f => f.UserId1 == userId1 && f.UserId2 == userId2);

            if (friendship != null)
            {
                if (friendship.Status == FriendshipStatus.Pending)
                    return ServiceResult<FriendshipDTO>.Failure(FriendshipErrorCode.FriendRequestAlreadyExists.GetDescription());

                if (friendship.Status == FriendshipStatus.Accepted)
                    return ServiceResult<FriendshipDTO>.Failure(FriendshipErrorCode.AlreadyFriends.GetDescription());

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

        public async Task<ServiceResult<FriendshipDTO>> UpdateFriendShipStatus(Guid userId, Guid friendshipId, FriendshipStatus status)
        {
            if (userId == Guid.Empty)
                return ServiceResult<FriendshipDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (friendshipId == Guid.Empty)
                return ServiceResult<FriendshipDTO>.Failure(FriendshipErrorCode.FriendshipIdRequired.GetDescription());

            var friendship = await _dbContext.Friendships
                .FirstOrDefaultAsync(f => f.Id == friendshipId &&
                    (f.UserId1 == userId || f.UserId2 == userId)
                );

            if (friendship is null)
                return ServiceResult<FriendshipDTO>.Failure(
                    FriendshipErrorCode.FriendshipNotFound.GetDescription(), HttpStatusCode.NotFound
                );

            string? statusError = StatusValidation(userId, friendship, status);
            if (statusError != null)
                return ServiceResult<FriendshipDTO>.Failure(statusError);

            friendship.Status = status;
            friendship.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return await GetFriendDTO(friendship, userId);
        }
    }
}
