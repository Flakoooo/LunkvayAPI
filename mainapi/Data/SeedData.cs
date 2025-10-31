using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.Data
{
    public static class SeedData
    {
        public static async Task Initialize(LunkvayDBContext context)
        {
            string userException = "Необходимый пользователь не найден. Проверьте базу данных";
            string userChat = "Необходимый чат не найден. Проверьте базу данных";

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

            if (usersExist)
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

                var profilesToEnsure = new[]
                {
                    new { User = userRyan, Status = "Будь настоящим примером для подражания,будь настоящим героем", About = "A Real Hero" },
                    new { User = userRinat, Status = "Будь настоящим примером для героя,будь настоящим подражанием", About = "A Real Being" },
                    new { User = userChristian, Status = "Perfet Girl", About = "Бэтмэн, гонщик, психопат" },
                    new { User = userTom, Status = "Я сюда припёрся, чтобы нормально пострелять! Ждал нормальной перестрелки с нормальными мужиками", About = "Безумный веном Бэйн" },
                    new { User = userJake, Status = "Истинная цена 'чего-то' сводится к тому, сколько за 'что-то' готовы дать.", About = "Я швед, я Йилленхолл" }
                };

                foreach (var profileData in profilesToEnsure)
                {
                    bool profileExists = await context.Profiles.AnyAsync(p => p.UserId == profileData.User.Id);
                    if (!profileExists)
                    {
                        await context.Profiles.AddAsync(new Profile
                        {
                            UserId = profileData.User.Id,
                            Status = profileData.Status,
                            About = profileData.About
                        });
                    }
                }

                await context.SaveChangesAsync();
            }

            if (!await context.Friendships.AnyAsync() && usersExist)
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

            if (!await context.Chats.AnyAsync() && usersExist)
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

                Chat personalChatRyanAndRinat = new()
                {
                    CreatorId = null,
                    LastMessageId = null,
                    Name = null,
                    Type = ChatType.Personal,
                };
                Chat groupChat = new()
                {
                    CreatorId = userRyan.Id,
                    LastMessageId = null,
                    Name = "Барбарики",
                    Type = ChatType.Group
                };
                await context.Chats.AddRangeAsync(
                    personalChatRyanAndRinat, groupChat
                );
                await context.SaveChangesAsync();

                DbSet<Chat> chats = context.Chats;
                personalChatRyanAndRinat = await chats.FirstOrDefaultAsync(c => c.Type == ChatType.Personal)
                    ?? throw new Exception(userChat);
                groupChat = await chats.FirstOrDefaultAsync(c => c.Type == ChatType.Group)
                    ?? throw new Exception(userChat);
                await context.ChatMembers.AddRangeAsync(
                    new ChatMember
                    {
                        ChatId = personalChatRyanAndRinat.Id,
                        MemberId = userRyan.Id,
                        MemberName = "Драйвер",
                        Role = ChatMemberRole.Member
                    },
                    new ChatMember
                    {
                        ChatId = personalChatRyanAndRinat.Id,
                        MemberId = userRinat.Id,
                        MemberName = null,
                        Role = ChatMemberRole.Member
                    },
                    new ChatMember
                    {
                        ChatId = groupChat.Id,
                        MemberId = userRyan.Id,
                        MemberName = "Бегущий по лезвию",
                        Role = ChatMemberRole.Owner
                    },
                    new ChatMember
                    {
                        ChatId = groupChat.Id,
                        MemberId = userRinat.Id,
                        MemberName = null,
                        Role = ChatMemberRole.Member
                    },
                    new ChatMember
                    {
                        ChatId = groupChat.Id,
                        MemberId = userChristian.Id,
                        MemberName = null,
                        Role = ChatMemberRole.Administrator
                    },
                    new ChatMember
                    {
                        ChatId = groupChat.Id,
                        MemberId = userTom.Id,
                        MemberName = "Я - Веном",
                        Role = ChatMemberRole.Member
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
