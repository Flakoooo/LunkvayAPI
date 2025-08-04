using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Enums;

namespace LunkvayAPI.src.Utils
{
    public static class SeedData
    {
        private static User CreateUser(
            string email, 
            string userName, 
            string password, 
            string firstName, 
            string lastName, 
            bool isDeleted = false, 
            bool isActive = true
        ) => new() 
        {
            Email = email,
            UserName = userName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = firstName,
            LastName = lastName,
            //CreatedAt в базе данных по UTC
            IsDeleted = isDeleted,
            //DeletedAt nullable
            //LastLogin в базе данных по UTC
            IsActive = isActive
        };

        public static void Initialize(LunkvayDBContext context)
        {
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    CreateUser("ryan.gosling@gmail.com", "ryangosling", "realhero", "Райан", "Гослинг"),
                    CreateUser("rinat.goslinov@gmail.com", "rinatgoslinov", "rialhero", "Ринат", "Гослинов"),
                    CreateUser("christian.bale@gmail.com", "christianbale", "fordvsferrari", "Кристиан", "Бейл"),
                    CreateUser("tom.hardy@gmail.com", "tomhardy", "madmax", "Том", "Харди"),
                    CreateUser("jake.gyllenhaal@gmail.com", "jakegyllenhaal", "mystereo", "Джейк", "Джилленхол")
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
                var users = context.Users;
                var userRyan = users.Where(u => u.Email == "ryan.gosling@gmail.com").First();
                var userRinat = users.Where(u => u.Email == "rinat.goslinov@gmail.com").First();
                var userChristian = users.Where(u => u.Email == "christian.bale@gmail.com").First();
                context.Avatars.AddRange(
                    new Avatar { UserId = userRyan.Id, FileName = $"{userRyan.UserName}.jpeg" },
                    new Avatar { UserId = userRinat.Id, FileName = $"{userRinat.UserName}.jpg" },
                    new Avatar { UserId = userChristian.Id, FileName = $"{userChristian.UserName}.jpg" }
                );
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
    }
}
