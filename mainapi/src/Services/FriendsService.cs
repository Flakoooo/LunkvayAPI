using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Enums;
using LunkvayAPI.src.Services.Interfaces;
using LunkvayAPI.src.Utils;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.src.Services
{
    public class FriendsService(
        ILogger<FriendsService> logger,
        LunkvayDBContext lunkvayDBContext
        ) : IFriendsService
    {
        /*
        private readonly List<Friendship> _relationships = 
            [
                new Friendship { UserId1 = Guid.NewGuid(), UserId2 = Guid.NewGuid(), Status = FriendshipStatus.Accepted, InitiatorId = "2", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Friendship { UserId1 = Guid.NewGuid(), UserId2 = Guid.NewGuid(), Status = FriendshipStatus.Pending, InitiatorId = "4", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Friendship { UserId1 = Guid.NewGuid(), UserId2 = Guid.NewGuid(), Status = FriendshipStatus.Accepted, InitiatorId = "3", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Friendship { UserId1 = Guid.NewGuid(), UserId2 = Guid.NewGuid(), Status = FriendshipStatus.Accepted, InitiatorId = "1", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            ];
        */

        private readonly ILogger<FriendsService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        //короче штука для постепенного прогруза, надо будет чекнуть
        //public async Task<List<User>> GetUserFriends(string userId, int skip = 0, int take = 10)
        public async Task<IEnumerable<UserDTO>> GetUserFriends(Guid userId)
        {
            _logger.LogInformation("({Date}) Осуществляется вывод друзей для {Id}", DateTime.Now, userId);

            var friendIds = await _dbContext.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted && (f.UserId1 == userId || f.UserId2 == userId))
                .Select(f => f.UserId1 == userId ? f.UserId2 : f.UserId1)
                .ToListAsync();

            var friends = await _dbContext.Users
                .Where(u => friendIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => new UserDTO
                {
                    Id = u.Id.ToString(),
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .ToListAsync();

            _logger.LogInformation("({Date}) Всего друзей {Count}", DateTime.Now, friends.Count);
            return friends;
            //return friends.Skip(skip).Take(take).ToList();
        }
    }
}
