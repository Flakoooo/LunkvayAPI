using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Enums;

namespace LunkvayAPI.src.Utils
{
    public static class SeedData
    {
        public static void Initialize(LunkvayDBContext context)
        {
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User { Email = "ryan.gosling@gmail.com", UserName = "ryangosling", PasswordHash = HashPassword("realhero"), FirstName = "Райан", LastName = "Гослинг" },
                    new User { Email = "rinat.goslinov@gmail.com", UserName = "rinatgoslinov", PasswordHash = HashPassword("realhero"), FirstName = "Ринат", LastName = "Гослинов" },
                    new User { Email = "christian.bale@gmail.com", UserName = "christianbale", PasswordHash = HashPassword("fordvsferrari"), FirstName = "Кристиан", LastName = "Бейл" },
                    new User { Email = "tom.hardy@gmail.com", UserName = "tomhardy", PasswordHash = HashPassword("madmax"), FirstName = "Том", LastName = "Харди" },
                    new User { Email = "jake.gyllenhaal@gmail.com", UserName = "jakegyllenhaal", PasswordHash = HashPassword("realhero"), FirstName = "Джейк", LastName = "Джилленхол" }
                );
                context.SaveChanges();
            }

            if (!context.Friendships.Any() && context.Users.Any())
            {
                var users = context.Users;
                var userRyan = users.Where(u => u.Email == "ryan.gosling@gmail.com").First();
                var userRinat = users.Where(u => u.Email == "rinat.goslinov@gmail.com").First();
                var userChristian = users.Where(u => u.Email == "christian.bale@gmail.com").First();
                var userTom = users.Where(u => u.Email == "tom.hardy@gmail.com").First();
                var userJake = users.Where(u => u.Email == "jake.gyllenhaal@gmail.com").First();
                context.Friendships.AddRange(
                    new Friendship { UserId1 = userRyan.Id, UserId2 = userRinat.Id, Status = FriendshipStatus.Accepted, InitiatorId = userRinat.Id },
                    new Friendship { UserId1 = userChristian.Id, UserId2 = userTom.Id, Status = FriendshipStatus.Accepted, InitiatorId = userTom.Id },
                    new Friendship { UserId1 = userJake.Id, UserId2 = userRyan.Id, Status = FriendshipStatus.Rejected, InitiatorId = userJake.Id },
                    new Friendship { UserId1 = userChristian.Id, UserId2 = userRyan.Id, Status = FriendshipStatus.Accepted, InitiatorId = userRyan.Id },
                    new Friendship { UserId1 = userRyan.Id, UserId2 = userTom.Id, Status = FriendshipStatus.Pending, InitiatorId = userTom.Id }
                );
                context.SaveChanges();
            }

            if (!context.Avatars.Any() && context.Users.Any())
            {
                var user = context.Users.Where(u => u.Email == "ryan.gosling@gmail.com").First();
                context.Avatars.Add(new Avatar { UserId = user.Id, FileName = "profile.jpeg" });
                context.SaveChanges();
            }

            if (!context.Profiles.Any() && context.Users.Any())
            {
                var user = context.Users.Where(u => u.Email == "ryan.gosling@gmail.com").First();
                context.Profiles.AddRange(
                    new UserProfile { UserId = user.Id, About = "Зачем меня просят рассказать о слове 'себе'?", Status = "Зачем меня просят рассказать о слове 'себе'?"}
                );
                context.SaveChanges();
            }
        }

        private static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    }
}
