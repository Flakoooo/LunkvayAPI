using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Enums;
using LunkvayAPI.src.Services.Interfaces;
using LunkvayAPI.src.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LunkvayAPI.src.Services
{
    public class FriendsService(
        ILogger<FriendsService> logger, 
        LunkvayDBContext lunkvayDBContext
        ) : IFriendsService
    {
        private readonly ILogger<FriendsService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        private static UserListItemDTO BuildUserListItemDTO(User user) => new()
        {
            UserId = user.Id.ToString(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsOnline = true
        };

        public async Task<IEnumerable<UserListItemDTO>> GetUserFriends(Guid userId, int page, int pageSize)
        {
            _logger.LogInformation(
                "({Date}) Запрос друзей пользователя {Id} (страница {Page}, размер {PageSize})", 
                DateTime.UtcNow, userId, page, pageSize
                );

            var skip = (page - 1) * pageSize;

            var friendIds = await _dbContext.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted && (f.UserId1 == userId || f.UserId2 == userId))
                .Select(f => f.UserId1 == userId ? f.UserId2 : f.UserId1)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var friends = await _dbContext.Users
                .Where(u => friendIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => BuildUserListItemDTO(u))
                .ToListAsync();

            _logger.LogInformation("({Date}) Получено {Count} друзей", DateTime.UtcNow, friendIds.Count);
            return friends;
        }

        public async Task<(IEnumerable<UserListItemDTO> Friends, int FriendsCount)> GetRandomUserFriends(Guid userId, int count)
        {
            _logger.LogInformation("({Date}) Запрос друзей пользователя {Id} для профиля", DateTime.UtcNow, userId);

            var friendIds = await _dbContext.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted && (f.UserId1 == userId || f.UserId2 == userId))
                .Select(f => f.UserId1 == userId ? f.UserId2 : f.UserId1)
                .ToListAsync();

            IEnumerable<UserListItemDTO> friends = [];
            if (friendIds.Count < 6)
            {
                friends = await _dbContext.Users
                    .Where(u => friendIds.Contains(u.Id) && !u.IsDeleted)
                    .Select(u => BuildUserListItemDTO(u))
                    .ToListAsync();

                _logger.LogInformation("({Date}) Получено {Count} друзей", DateTime.UtcNow, friendIds.Count);
                return (friends, friendIds.Count);
            }

            var random = new Random();
            var randomFriendIds = friendIds
                .OrderBy(x => random.Next())
                .Take(count)
                .ToList();

            friends = await _dbContext.Users
                .Where(u => friendIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => BuildUserListItemDTO(u))
                .ToListAsync();

            _logger.LogInformation("({Date}) Получено {Count} друзей", DateTime.UtcNow, randomFriendIds.Count);
            return (friends, friendIds.Count);
        }
    }
}
