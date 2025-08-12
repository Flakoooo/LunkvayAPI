using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Entities.ChatAPI;
using LunkvayAPI.src.Models.Entities.FriendsAPI;
using LunkvayAPI.src.Models.Enums;
using LunkvayAPI.src.Models.Enums.ChatEnum;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.src.Utils
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
                    Friendship.Create(userRyan.Id, userRinat.Id, FriendshipStatus.Accepted, userRinat.Id),
                    Friendship.Create(userChristian.Id, userTom.Id, FriendshipStatus.Accepted, userTom.Id),
                    Friendship.Create(userJake.Id, userRyan.Id, FriendshipStatus.Rejected, userJake.Id),
                    Friendship.Create(userChristian.Id, userRyan.Id, FriendshipStatus.Accepted, userRyan.Id),
                    Friendship.Create(userRyan.Id, userTom.Id, FriendshipStatus.Pending, userTom.Id)
                );
                await context.SaveChangesAsync();
            }

            if (!await context.Avatars.AnyAsync() && usersExist)
            {
                DbSet<User> users = context.Users;

                userRyan ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(ryanGoslingEmail))
                    ?? throw new Exception(userException);
                userRinat ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(rinatGoslinovEmail))
                    ?? throw new Exception(userException);
                userChristian ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(christianBaleEmail))
                    ?? throw new Exception(userException);

                await context.Avatars.AddRangeAsync(
                    Avatar.Create(userRyan.Id, $"{userRyan.UserName}.jpeg"),
                    Avatar.Create(userRinat.Id, $"{userRinat.UserName}.jpg"),
                    Avatar.Create(userChristian.Id, $"{userChristian.UserName}.jpg")
                );
                await context.SaveChangesAsync();
            }

            if (!await context.Profiles.AnyAsync() && usersExist)
            {
                DbSet<User> users = context.Users;
                userRyan ??= await users.FirstOrDefaultAsync(u => u.Email.Equals(ryanGoslingEmail))
                    ?? throw new Exception(userException);
                await context.Profiles.AddRangeAsync(
                    UserProfile.Create(userRyan.Id, "Зачем меня просят рассказать о слове 'себе'?", "Зачем меня просят рассказать о слове 'себе'?")
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

                Chat personalChatRyanAndRinat = Chat.Create(null, null, null, null, ChatType.Personal);
                Chat groupChat = Chat.Create(userRyan.Id, null, "Барбарики", null, ChatType.Group);
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
                    ChatMember.Create(personalChatRyanAndRinat.Id, userRyan.Id, "Драйвер", ChatMemberRole.Member),
                    ChatMember.Create(personalChatRyanAndRinat.Id, userRinat.Id, null, ChatMemberRole.Member),
                    ChatMember.Create(groupChat.Id, userRyan.Id, "Бегущий по лезвию", ChatMemberRole.Owner),
                    ChatMember.Create(groupChat.Id, userRinat.Id, null, ChatMemberRole.Member),
                    ChatMember.Create(groupChat.Id, userChristian.Id, null, ChatMemberRole.Administrator),
                    ChatMember.Create(groupChat.Id, userTom.Id, "Я - Веном", ChatMemberRole.Member)
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
