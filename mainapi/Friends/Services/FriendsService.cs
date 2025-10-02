using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.Friends.Services
{
    public class FriendsService(
        ILogger<FriendsService> logger, 
        LunkvayDBContext lunkvayDBContext
    ) : IFriendsService
    {
        private readonly ILogger<FriendsService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        public async Task<ServiceResult<IEnumerable<UserListItemDTO>>> GetUserFriends(Guid userId, int page, int pageSize)
        {
            _logger.LogInformation(
                "({Date}) Запрос друзей пользователя {Id} (страница {Page}, размер {PageSize})", 
                DateTime.UtcNow, userId, page, pageSize
            );

            List<Guid> friendIds = await _dbContext.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted && (f.UserId1 == userId || f.UserId2 == userId))
                .Select(f => f.UserId1 == userId ? f.UserId2 : f.UserId1)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<UserListItemDTO> friends = await _dbContext.Users
                .Where(u => friendIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => new UserListItemDTO
                {
                    UserId = u.Id,
                    FirstName = u.FirstName,
                    IsOnline = u.IsOnline,
                    LastName = u.LastName
                })
                .ToListAsync();

            _logger.LogInformation("({Date}) Получено {Count} друзей", DateTime.UtcNow, friendIds.Count);
            return ServiceResult<IEnumerable<UserListItemDTO>>.Success(friends);
        }

        public async Task<ServiceResult<(IEnumerable<UserListItemDTO> Friends, int FriendsCount)>> GetRandomUserFriends(Guid userId, int count)
        {
            _logger.LogInformation("({Date}) Запрос друзей пользователя {Id} для профиля", DateTime.UtcNow, userId);

            List<Guid> friendIds = await _dbContext.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted && (f.UserId1 == userId || f.UserId2 == userId))
                .Select(f => f.UserId1 == userId ? f.UserId2 : f.UserId1)
                .ToListAsync();

            List<UserListItemDTO> friends = [];
            if (friendIds.Count <= count)
            {
                friends = await _dbContext.Users
                    .Where(u => friendIds.Contains(u.Id) && !u.IsDeleted)
                    .Select(u => new UserListItemDTO 
                    { 
                        UserId = u.Id, 
                        FirstName = u.FirstName, 
                        IsOnline = u.IsOnline, 
                        LastName = u.LastName 
                    })
                    .ToListAsync();
            }
            else
            {
                Random random = new();
                List<Guid> randomFriendIds = [.. friendIds.OrderBy(x => random.Next()).Take(count)];

                friends = await _dbContext.Users
                    .Where(u => randomFriendIds.Contains(u.Id) && !u.IsDeleted)
                    .Select(u => new UserListItemDTO
                    {
                        UserId = u.Id,
                        FirstName = u.FirstName,
                        IsOnline = u.IsOnline,
                        LastName = u.LastName
                    })
                    .ToListAsync();
            }

            _logger.LogInformation("({Date}) Получено {Count} друзей, всего {CountAll} друзей", DateTime.UtcNow, friends.Count, friendIds.Count);
            return ServiceResult<(IEnumerable<UserListItemDTO> Friends, int FriendsCount)>.Success((friends, friendIds.Count));
        }
    }
}
