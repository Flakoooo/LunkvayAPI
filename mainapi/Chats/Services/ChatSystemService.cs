using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.Chats.Services
{
    public class ChatSystemService(
        LunkvayDBContext lunkvayDBContext
    ) : IChatSystemService
    {
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        public async Task<ServiceResult<Chat>> GetChat(Guid chatId)
        {
            if (chatId == Guid.Empty)
                return ServiceResult<Chat>.Failure("Id чата не может быть пустым");

            var chat = await _dbContext.Chats
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat is null)
                return ServiceResult<Chat>.Failure("Чат не найден");

            return ServiceResult<Chat>.Success(chat);
        }

        public async Task<Guid?> FindPersonalChatBetweenUsers(
            Guid user1Id, Guid user2Id
        ) => await _dbContext.ChatMembers
            .Where(cm1 => cm1.MemberId == user1Id && !cm1.IsDeleted)
            .Join(_dbContext.ChatMembers.Where(cm2 => cm2.MemberId == user2Id && !cm2.IsDeleted),
                cm1 => cm1.ChatId,
                cm2 => cm2.ChatId,
                (cm1, cm2) => cm1.ChatId)
            .Join(_dbContext.Chats.Where(c => c.Type == ChatType.Personal),
                chatId => chatId,
                chat => chat.Id,
                (chatId, chat) => chatId)
            .FirstOrDefaultAsync();

        public async Task<ServiceResult<Chat>> CreatePersonalChat(
            ChatType chatType, string? name
        )
        {
            if (chatType != ChatType.Personal)
                return ServiceResult<Chat>.Failure("Метод предназначен только для личных чатов");

            var chat = new Chat
            {
                CreatorId = null,
                Name = name,
                Type = chatType
            };

            await _dbContext.Chats.AddAsync(chat);
            await _dbContext.SaveChangesAsync();

            return ServiceResult<Chat>.Success(chat);
        }

        public async Task<ServiceResult<Chat>> UpdateChatLastMessage(
            Guid chatId, Guid lastMessageId
        )
        {
            if (chatId == Guid.Empty)
                return ServiceResult<Chat>.Failure("Id чата не может быть пустым");

            var chat = await _dbContext.Chats
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat is null)
                return ServiceResult<Chat>.Failure("Id чат не найден");

            chat.LastMessageId = lastMessageId;
            chat.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return ServiceResult<Chat>.Success(chat);
        }
    }
}
