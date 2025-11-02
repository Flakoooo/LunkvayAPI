using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.Chats.Services
{
    public class ChatSystemService(
        ILogger<ChatSystemService> logger,
        LunkvayDBContext lunkvayDBContext
    ) : IChatSystemService
    {
        private readonly ILogger<ChatSystemService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        public async Task<ServiceResult<Chat>> GetChatBySystem(Guid chatId)
        {
            if (chatId == Guid.Empty)
                return ServiceResult<Chat>.Failure("Id чата не может быть пустым");

            var chat = await _dbContext.Chats
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat is null)
                return ServiceResult<Chat>.Failure("Id чат не найден");

            return ServiceResult<Chat>.Success(chat);
        }

        public async Task<ServiceResult<Chat>> UpdateChatLastMessageBySystem(
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
