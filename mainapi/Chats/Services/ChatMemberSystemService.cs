using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace LunkvayAPI.Chats.Services
{
    public class ChatMemberSystemService(
        ILogger<ChatMemberSystemService> logger,
        LunkvayDBContext lunkvayDBContext
    ) : IChatMemberSystemService
    {
        private readonly ILogger<ChatMemberSystemService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        public async Task<bool> ExistAnyChatMembersBySystem(Expression<Func<ChatMember, bool>> predicate)
            => await _dbContext.ChatMembers.AsNoTracking().AnyAsync(predicate);

        public async Task<List<ChatMember>> GetChatMembersByChatIdBySystem(Guid chatId)
            => await _dbContext.ChatMembers
                .Where(cm => cm.ChatId == chatId)
                .ToListAsync();

        public async Task<ServiceResult<ChatMember>> CreateMemberBySystem(
            Guid chatId, Guid memberId, ChatMemberRole role
        )
        {
            if (chatId == Guid.Empty)
                return ServiceResult<ChatMember>.Failure("Id чата не может быть пустым");

            if (memberId == Guid.Empty)
                return ServiceResult<ChatMember>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var chatMember
                = new ChatMember { ChatId = chatId, MemberId = memberId, Role = role };

            await _dbContext.ChatMembers.AddAsync(chatMember);

            await _dbContext.SaveChangesAsync();

            return ServiceResult<ChatMember>.Success(chatMember);
        }

        public async Task<ServiceResult<List<ChatMember>>> CreatePersonalMembersBySystem(
            Guid chatId, Guid memberId1, Guid memberId2
        )
        {
            if (chatId == Guid.Empty)
                return ServiceResult<List<ChatMember>>.Failure("Id чата не может быть пустым");

            if (memberId1 == Guid.Empty || memberId2 == Guid.Empty)
                return ServiceResult<List<ChatMember>>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var members = new List<ChatMember>()
            {
                new()
                {
                    ChatId = chatId,
                    MemberId = memberId1,
                    Role = ChatMemberRole.Member
                },
                new()
                {
                    ChatId = chatId,
                    MemberId = memberId2,
                    Role = ChatMemberRole.Member
                }
            };

            await _dbContext.ChatMembers.AddRangeAsync(members);

            await _dbContext.SaveChangesAsync();

            return ServiceResult<List<ChatMember>>.Success(members);
        }

        public async Task<ServiceResult<List<ChatMember>>> CreateGroupMembersBySystem(
            Guid chatId, Guid creatorId, IList<Guid> members
        )
        {
            var chatMembers = new List<ChatMember>();

            if (members.Any(guid => guid == creatorId))
                members.Remove(members.First(guid => guid == creatorId));

            chatMembers.Add(new ChatMember
            {
                ChatId = chatId,
                MemberId = creatorId,
                Role = ChatMemberRole.Owner
            });

            foreach (var member in members.Where(guid => guid != creatorId))
            {
                chatMembers.Add(new ChatMember
                {
                    ChatId = chatId,
                    MemberId = member,
                    Role = ChatMemberRole.Member
                });
            }

            await _dbContext.ChatMembers.AddRangeAsync(chatMembers);
            await _dbContext.SaveChangesAsync();

            return ServiceResult<List<ChatMember>>.Success(chatMembers);
        }
    }
}
