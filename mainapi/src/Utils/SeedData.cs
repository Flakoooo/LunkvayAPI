using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Entities.FriendsAPI;
using LunkvayAPI.src.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.src.Utils
{
    public static class SeedData
    {
        public static void Initialize(LunkvayDBContext context)
        {
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    User.Create("ryangosling", "ryan.gosling@gmail.com", "realhero", "Райан", "Гослинг"),
                    User.Create("rinatgoslinov", "rinat.goslinov@gmail.com", "rialhero", "Ринат", "Гослинов"),
                    User.Create("christianbale", "christian.bale@gmail.com", "fordvsferrari", "Кристиан", "Бейл"),
                    User.Create("tomhardy", "tom.hardy@gmail.com", "madmax", "Том", "Харди"),
                    User.Create("jakegyllenhaal", "jake.gyllenhaal@gmail.com", "mystereo", "Джейк", "Джилленхол")
                );
                _ = context.SaveChanges();
            }

            if (!context.Friendships.Any() && context.Users.Any())
            {
                DbSet<User> users = context.Users;
                User userRyan = users.Where(static u => u.Email == "ryan.gosling@gmail.com").First();
                User userRinat = users.Where(static u => u.Email == "rinat.goslinov@gmail.com").First();
                User userChristian = users.Where(static u => u.Email == "christian.bale@gmail.com").First();
                User userTom = users.Where(static u => u.Email == "tom.hardy@gmail.com").First();
                User userJake = users.Where(static u => u.Email == "jake.gyllenhaal@gmail.com").First();
                context.Friendships.AddRange(
                    Friendship.Create(userRyan.Id, userRinat.Id, FriendshipStatus.Accepted, userRinat.Id),
                    Friendship.Create(userChristian.Id, userTom.Id, FriendshipStatus.Accepted, userTom.Id),
                    Friendship.Create(userJake.Id, userRyan.Id, FriendshipStatus.Rejected, userJake.Id),
                    Friendship.Create(userChristian.Id, userRyan.Id, FriendshipStatus.Accepted, userRyan.Id),
                    Friendship.Create(userRyan.Id, userTom.Id, FriendshipStatus.Pending, userTom.Id)
                );
                _ = context.SaveChanges();
            }

            if (!context.Avatars.Any() && context.Users.Any())
            {
                DbSet<User> users = context.Users;
                User userRyan = users.Where(static u => u.Email == "ryan.gosling@gmail.com").First();
                User userRinat = users.Where(static u => u.Email == "rinat.goslinov@gmail.com").First();
                User userChristian = users.Where(static u => u.Email == "christian.bale@gmail.com").First();
                context.Avatars.AddRange(
                    Avatar.Create(userRyan.Id, $"{userRyan.UserName}.jpeg"),
                    Avatar.Create(userRinat.Id, $"{userRinat.UserName}.jpg"),
                    Avatar.Create(userChristian.Id, $"{userChristian.UserName}.jpg")
                );
                _ = context.SaveChanges();
            }

            if (!context.Profiles.Any() && context.Users.Any())
            {
                DbSet<User> users = context.Users;
                User userRyan = users.Where(static u => u.Email == "ryan.gosling@gmail.com").First();
                context.Profiles.AddRange(
                    UserProfile.Create(userRyan.Id, "Зачем меня просят рассказать о слове 'себе'?", "Зачем меня просят рассказать о слове 'себе'?")
                );
                _ = context.SaveChanges();
            }
        }
    }
}
