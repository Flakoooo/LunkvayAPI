using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Enums;
using LunkvayAPI.src.Services.Interfaces;

namespace LunkvayAPI.src.Services
{
    public class FriendsService : IFriendsService
    {
        private readonly List<Relationships> _relationships = 
            [
                new Relationships { UserId1 = "2", UserId2 = "1", Status = RelationshipsStatus.Accepted, InitiatorId = "2", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Relationships { UserId1 = "4", UserId2 = "1", Status = RelationshipsStatus.Pending, InitiatorId = "4", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Relationships { UserId1 = "3", UserId2 = "1", Status = RelationshipsStatus.Accepted, InitiatorId = "3", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Relationships { UserId1 = "1", UserId2 = "5", Status = RelationshipsStatus.Accepted, InitiatorId = "1", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            ];

        private readonly IUserService _userService;
        private readonly ILogger<FriendsService> _logger;

        public FriendsService(IUserService userService, ILogger<FriendsService> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        //короче штука для постепенного прогруза, надо будет чекнуть
        //public async Task<List<User>> GetUserFriends(string userId, int skip = 0, int take = 10)
        public async Task<IEnumerable<UserDTO>> GetUserFriends(string userId)
        {
            _logger.LogInformation("({Date}) Осуществляется вывод друзей для {Id}", DateTime.Now, userId);
            var acceptedRelationships = _relationships
                .Where(r => r.Status == RelationshipsStatus.Accepted && (r.UserId1 == userId || r.UserId2 == userId))
                .ToList();

            var friendTasks = acceptedRelationships
                .Select(async relationship =>
                {
                    string friendId = relationship.UserId1 == userId
                        ? relationship.UserId2
                        : relationship.UserId1;
                    User user = await _userService.GetUserById(friendId);
                    return UserDTO.ConvertUserToDTO(user);
                });

            var friends = await Task.WhenAll(friendTasks);
            _logger.LogInformation("({Date}) Всего друзей {Count}", DateTime.Now, friends.Length);
            //return friends.Skip(skip).Take(take).ToList();
            return [.. friends];
        }
    }
}
