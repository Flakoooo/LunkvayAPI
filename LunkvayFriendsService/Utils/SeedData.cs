using LunkvayAPI.src.Models.Entities.FriendsAPI;
using LunkvayFriendsService.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LunkvayFriendsService.Utils
{
    public static class SeedData
    {
        public static async Task Initialize(LunkvayDBContext context)
        {
            string userException = "Необходимый пользователь не найден. Проверьте базу данных";

            string ryanGoslingEmail = "ryan.gosling@gmail.com";
            string rinatGoslinovEmail = "rinat.goslinov@gmail.com";
            string christianBaleEmail = "christian.bale@gmail.com";
            string tomHardyEmail = "tom.hardy@gmail.com";
            string jakeGyllenhaalEmail = "jake.gyllenhaal@gmail.com";

            User? userRyan = null;
            User? userRinat = null;
            User? userChristian = null;
            User? userTom = null;
            User? userJake = null;
            if (!await context.Friendships.AnyAsync())
            {
                DbSet<User> users = context.Users;
                userRyan ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(ryanGoslingEmail))
                    ?? throw new Exception(userException);
                userRinat ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(rinatGoslinovEmail))
                    ?? throw new Exception(userException);
                userChristian ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(christianBaleEmail))
                    ?? throw new Exception(userException);
                userTom ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(tomHardyEmail))
                    ?? throw new Exception(userException);
                userJake ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(jakeGyllenhaalEmail))
                    ?? throw new Exception(userException);

                await context.Friendships.AddRangeAsync(
                    new Friendship
                    {
                        UserId1 = userRyan.Id,
                        UserId2 = userRinat.Id,
                        Status = FriendshipStatus.Accepted,
                        InitiatorId = userRyan.Id,
                    },
                    new Friendship
                    {
                        UserId1 = userChristian.Id,
                        UserId2 = userTom.Id,
                        Status = FriendshipStatus.Accepted,
                        InitiatorId = userTom.Id,
                    },
                    new Friendship
                    {
                        UserId1 = userJake.Id,
                        UserId2 = userRyan.Id,
                        Status = FriendshipStatus.Rejected,
                        InitiatorId = userJake.Id,
                    },
                    new Friendship
                    {
                        UserId1 = userChristian.Id,
                        UserId2 = userRyan.Id,
                        Status = FriendshipStatus.Accepted,
                        InitiatorId = userRyan.Id,
                    },
                    new Friendship
                    {
                        UserId1 = userRyan.Id,
                        UserId2 = userTom.Id,
                        Status = FriendshipStatus.Pending,
                        InitiatorId = userTom.Id,
                    }

                );
                await context.SaveChangesAsync();
            }
        }
    }
}
