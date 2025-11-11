using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Services
{
    public class ChatMessageSystemService(
        ILogger<ChatMessageSystemService> logger,
        LunkvayDBContext lunkvayDBContext,
        IChatNotificationService chatNotificationService
    ) : IChatMessageSystemService
    {
        private readonly ILogger<ChatMessageSystemService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly IChatNotificationService _chatNotificationService = chatNotificationService;

        private static ChatMessageDTO MapToDto(
            ChatMessage message,
            Guid? senderid, string? userName, string? firstName, string? LastName, bool? isOnline,
            bool isCurrentUser
        ) => new()
        {
            Id = message.Id,
            SenderId = senderid,
            SenderUserName = userName,
            SenderFirstName = firstName,
            SenderLastName = LastName,
            SenderIsOnline = isOnline,
            SystemMessageType = message.SystemMessageType,
            Message = message.Message,
            IsEdited = message.IsEdited,
            IsPinned = message.IsPinned,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt,
            PinnedAt = message.PinnedAt,
            IsMyMessage = isCurrentUser
        };

        public async Task<ServiceResult<ChatMessage>> CreateSystemChatMessage(
            Guid chatId, string message, SystemMessageType type
        )
        {
            var newMessage = new ChatMessage
            {
                ChatId = chatId,
                Message = message,
                SystemMessageType = type,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.ChatMessages.AddAsync(newMessage);

            await _dbContext.SaveChangesAsync();

            await _chatNotificationService.SendMessage(
                chatId, 
                MapToDto(
                    newMessage,
                    null, null, null, null, null,
                    false
                )
            );

            return ServiceResult<ChatMessage>.Success(newMessage);
        }
    }
}
