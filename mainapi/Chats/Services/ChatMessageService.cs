using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Users.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LunkvayAPI.Chats.Services
{
    public class ChatMessageService(
        ILogger<ChatMessageService> logger,
        IUserService userService,
        LunkvayDBContext lunkvayDBContext,
        IChatNotificationService chatNotificationService
    ) : IChatMessageService
    {
        private readonly ILogger<ChatMessageService> _logger = logger;
        private readonly IUserService _userService = userService;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly IChatNotificationService _chatNotificationService = chatNotificationService;

        public async Task<ServiceResult<IEnumerable<ChatMessageDTO>>> GetChatMessages(Guid userId, Guid chatId, int page, int pageSize)
        {
            _logger.LogInformation("({Date}) Запрос списка сообщений для пользователя {UserId} в чате {ChatId}", DateTime.Now, userId, chatId);

            List<ChatMessageDTO> chatMessages = await _dbContext.ChatMessages
                .Where(cm => cm.ChatId == chatId)
                .OrderBy(cm => cm.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cm => new ChatMessageDTO
                {
                    Id = cm.Id,
                    Sender = cm.Sender != null ? new UserDTO
                    {
                        Id = cm.Sender.Id,
                        UserName = cm.Sender.UserName,
                        FirstName = cm.Sender.FirstName,
                        LastName = cm.Sender.LastName,
                        Email = cm.Sender.Email,
                        CreatedAt = cm.Sender.CreatedAt,
                        IsDeleted = cm.Sender.IsDeleted,
                        LastLogin = cm.Sender.LastLogin,
                        IsOnline = cm.Sender.IsOnline
                    } : null,
                    SystemMessageType = cm.SystemMessageType,
                    Message = cm.Message,
                    IsEdited = cm.IsEdited,
                    IsPinned = cm.IsPinned,
                    CreatedAt = cm.CreatedAt,
                    UpdatedAt = cm.UpdatedAt,
                    IsMyMessage = cm.SenderId == userId
                })
                .ToListAsync();

            _logger.LogInformation("({Date}) Получено {Count} сообщений", DateTime.UtcNow, chatMessages.Count);

            return ServiceResult<IEnumerable<ChatMessageDTO>>.Success(chatMessages);
        }

        public async Task<ServiceResult<ChatMessageDTO>> CreateMessage(ChatMessageRequest chatMessageRequest, Guid senderId)
        {
            UserDTO sender;
            var senderResult = await _userService.GetUserById(senderId);
            if (senderResult.IsSuccess && senderResult.Result is not null)
                sender = senderResult.Result;
            else
                return ServiceResult<ChatMessageDTO>.Failure(
                    senderResult.Error ?? "Непредвиденная ошибка", 
                    HttpStatusCode.InternalServerError
                );

            ChatMessage chatMessage = new()
            {
                ChatId = chatMessageRequest.ChatId,
                SenderId = senderId,
                SystemMessageType = SystemMessageType.None,
                Message = chatMessageRequest.Message,
                IsEdited = false,
                IsPinned = false,
                IsDeleted = false,
                UpdatedAt = null,
                PinnedAt = null,
                DeletedAt = null
            };

            await _dbContext.AddAsync(chatMessage);
            await _dbContext.SaveChangesAsync();
            var chat = await _dbContext.Chats.FindAsync(chatMessageRequest.ChatId);
            if (chat != null)
            {
                chat.LastMessageId = chatMessage.Id;
                chat.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            ChatMessageDTO chatMessageDTO = new()
            {
                Id = chatMessage.Id,
                Sender = sender,
                SystemMessageType = SystemMessageType.None,
                Message = chatMessage.Message,
                IsEdited = chatMessage.IsEdited,
                IsPinned = chatMessage.IsPinned,
                CreatedAt = chatMessage.CreatedAt,
                UpdatedAt = chatMessage.UpdatedAt,
                IsMyMessage = true
            };

            await _chatNotificationService.SendMessage(
                chatMessageRequest.ChatId, chatMessageDTO
            );

            return ServiceResult<ChatMessageDTO>.Success(chatMessageDTO);
        }
    }
}
