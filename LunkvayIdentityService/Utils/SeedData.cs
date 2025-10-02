using LunkvayIdentityService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LunkvayIdentityService.Utils
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

            bool usersExist = await context.Users.AnyAsync();
            if (!usersExist)
            {
                await context.Users.AddRangeAsync(
                    User.Create("ryangosling", ryanGoslingEmail, "realhero", "Райан", "Гослинг"),
                    User.Create("rinatgoslinov", rinatGoslinovEmail, "rialhero", "Ринат", "Гослинов"),
                    User.Create("christianbale", christianBaleEmail, "fordvsferrari", "Кристиан", "Бейл"),
                    User.Create("tomhardy", tomHardyEmail, "madmax", "Том", "Харди"),
                    User.Create("jakegyllenhaal", jakeGyllenhaalEmail, "mystereo", "Джейк", "Джилленхол")
                );
                await context.SaveChangesAsync();
                usersExist = true;
            }

            if (!await context.Avatars.AnyAsync() && usersExist)
            {
                DbSet<User> users = context.Users;

                userRyan 
                    ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(ryanGoslingEmail))
                    ?? throw new Exception(userException);
                userRinat 
                    ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(rinatGoslinovEmail))
                    ?? throw new Exception(userException);
                userChristian 
                    ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(christianBaleEmail))
                    ?? throw new Exception(userException);

                await context.Avatars.AddRangeAsync(
                    new Avatar { UserId = userRyan.Id, FileName = $"{userRyan.UserName}.jpeg" },
                    new Avatar { UserId = userRinat.Id, FileName = $"{userRinat.UserName}.jpg" },
                    new Avatar { UserId = userChristian.Id, FileName = $"{userChristian.UserName}.jpg" }
                );
                await context.SaveChangesAsync();
            }

            if (!await context.Profiles.AnyAsync() && usersExist)
            {
                DbSet<User> users = context.Users;
                userRyan 
                    ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(ryanGoslingEmail))
                    ?? throw new Exception(userException);

                await context.Profiles.AddRangeAsync(
                    new UserProfile
                    {
                        UserId = userRyan.Id,
                        Status = "Зачем меня просят рассказать о слове 'себе'?",
                        About = "Зачем меня просят рассказать о слове 'себе'?"
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
