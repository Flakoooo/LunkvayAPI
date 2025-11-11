using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.Friends.Services
{
    public class FriendshipsSystemService(
        LunkvayDBContext lunkvayDBContext
    ) : IFriendshipsSystemService
    {
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        public async Task<RandomFriendsResult?> GetRandomFriends(Guid userId, int count)
        {
            if (userId == Guid.Empty)
                return null;

            var query = _dbContext.Friendships
                .AsNoTracking()
                .Where(f => 
                    f.Status == FriendshipStatus.Accepted 
                    && (f.UserId1 == userId || f.UserId2 == userId)
                );

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

            return new RandomFriendsResult { 
                Friends = friends, FriendsCount = friendsCount 
            };
        }
    }
}
