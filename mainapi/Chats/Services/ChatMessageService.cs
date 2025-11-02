using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
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
        LunkvayDBContext lunkvayDBContext,
        IUserService userService,
        IChatSystemService chatService,
        IChatMemberSystemService chatMemberService,
        IChatNotificationService chatNotificationService
    ) : IChatMessageService
    {
        private readonly ILogger<ChatMessageService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly IUserService _userService = userService;
        private readonly IChatSystemService _chatService = chatService;
        private readonly IChatMemberSystemService _chatMemberService = chatMemberService;
        private readonly IChatNotificationService _chatNotificationService = chatNotificationService;

        private static ChatMessageDTO MapToDto(
            ChatMessage message, UserDTO? userDTO, bool isCurrentUser
        ) => new()
        {
            Id = message.Id,
            Sender = userDTO,
            SystemMessageType = message.SystemMessageType,
            Message = message.Message,
            IsEdited = message.IsEdited,
            IsPinned = message.IsPinned,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt,
            IsMyMessage = isCurrentUser
        };


        public async Task<ServiceResult<List<ChatMessageDTO>>> GetChatMessages(Guid userId, Guid chatId, int page, int pageSize)
        {
            _logger.LogInformation("({Date}) Запрос списка сообщений для пользователя {UserId} в чате {ChatId}", DateTime.Now, userId, chatId);

            List<ChatMessageDTO> chatMessages = await _dbContext.ChatMessages
                .Where(cm => cm.ChatId == chatId && !cm.IsDeleted)
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

            return ServiceResult<List<ChatMessageDTO>>.Success(chatMessages);
        }

        public async Task<ServiceResult<List<ChatMessageDTO>>> GetPinnedChatMessages(Guid userId, Guid chatId, int page, int pageSize)
        {
            _logger.LogInformation("({Date}) Запрос списка сообщений для пользователя {UserId} в чате {ChatId}", DateTime.Now, userId, chatId);

            List<ChatMessageDTO> chatMessages = await _dbContext.ChatMessages
                .Where(cm => cm.ChatId == chatId && !cm.IsDeleted && cm.IsPinned)
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

            return ServiceResult<List<ChatMessageDTO>>.Success(chatMessages);
        }

        public async Task<ServiceResult<ChatMessageDTO>> CreateMessage(Guid senderId, CreateChatMessageRequest request)
        {
            if (senderId == Guid.Empty)
                return ServiceResult<ChatMessageDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (request.ChatId == Guid.Empty)
                return ServiceResult<ChatMessageDTO>.Failure("Id чата не может быть пустым");

            if (string.IsNullOrWhiteSpace(request.Message))
                return ServiceResult<ChatMessageDTO>.Failure("Сообщение не может быть пустым");

            if (!await _chatMemberService.ExistAnyChatMembersBySystem(cm => cm.ChatId == request.ChatId && cm.MemberId == senderId && !cm.IsDeleted))
                return ServiceResult<ChatMessageDTO>.Failure("Вы не являетесь участником этого чата", HttpStatusCode.Forbidden);

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var chatMessage = new ChatMessage
                {
                    ChatId = request.ChatId,
                    SenderId = senderId,
                    SystemMessageType = SystemMessageType.None,
                    Message = request.Message
                };

                await _dbContext.AddAsync(chatMessage);
                await _dbContext.SaveChangesAsync();

                var chat = await _chatService.UpdateChatLastMessageBySystem(
                    request.ChatId, chatMessage.Id
                );

                ServiceResult<UserDTO> senderResult = await _userService.GetUserById(senderId);
                var user = senderResult.Result;

                var chatMessageDTO = MapToDto(chatMessage, user, chatMessage.SenderId == senderId);

                await _chatNotificationService.SendMessage(request.ChatId, chatMessageDTO);
                await transaction.CommitAsync();

                return ServiceResult<ChatMessageDTO>.Success(chatMessageDTO);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при создании сообщения в чате {ChatId}", request.ChatId);
                return ServiceResult<ChatMessageDTO>.Failure("Ошибка при отправке сообщения");
            }
        }

        public async Task<ServiceResult<ChatMessageDTO>> EditChatMessage(
            Guid editorId, UpdateEditChatMessageRequest request
        )
        {
            if (editorId == Guid.Empty)
                return ServiceResult<ChatMessageDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (request.ChatId == Guid.Empty)
                return ServiceResult<ChatMessageDTO>.Failure("Id чата не может быть пустым");

            if (request.MessageId == Guid.Empty)
                return ServiceResult<ChatMessageDTO>.Failure("Id сообщения не может быть пустым");

            if (string.IsNullOrWhiteSpace(request.NewMessage))
                return ServiceResult<ChatMessageDTO>.Failure("Новое сообщение не может быть пустым");

            var message = await _dbContext.ChatMessages
                .FirstOrDefaultAsync(cm => cm.Id == request.MessageId);

            // если сообщение не существует
            if (message is null)
                return ServiceResult<ChatMessageDTO>.Failure(
                    "Сообщение не найдено", HttpStatusCode.NotFound
                );

            // если сообщение вообще в дургом чате
            if (message.ChatId != request.ChatId)
                return ServiceResult<ChatMessageDTO>.Failure(
                    "Отказано в доступе", HttpStatusCode.Forbidden
                );

            //изменять сообщение может только сам пользователь
            if (message.SenderId != editorId)
                return ServiceResult<ChatMessageDTO>.Failure(
                    "Отказано в доступе", HttpStatusCode.Forbidden
                );

            if (message.SystemMessageType != SystemMessageType.None)
                return ServiceResult<ChatMessageDTO>.Failure("Нельзя редактировать системные сообщения");

            message.Message = request.NewMessage.Trim();
            message.IsEdited = true;
            message.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return ServiceResult<ChatMessageDTO>.Success(
                MapToDto(
                    message, 
                    (await _userService.GetUserById(message.SenderId)).Result, 
                    message.SenderId == editorId
                )
            );
        }

        public async Task<ServiceResult<ChatMessageDTO>> PinChatMessage(
            Guid initiatorId, UpdatePinChatMessageRequest request
        )
        {
            if (initiatorId == Guid.Empty)
                return ServiceResult<ChatMessageDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (request.ChatId == Guid.Empty)
                return ServiceResult<ChatMessageDTO>.Failure("Id чата не может быть пустым");

            if (request.MessageId == Guid.Empty)
                return ServiceResult<ChatMessageDTO>.Failure("Id сообщения не может быть пустым");

            var message = await _dbContext.ChatMessages
                .FirstOrDefaultAsync(cm => cm.Id == request.MessageId);

            // если сообщение не существует
            if (message is null)
                return ServiceResult<ChatMessageDTO>.Failure(
                    "Сообщение не найдено", HttpStatusCode.NotFound
                );

            // если сообщение вообще в дургом чате
            if (message.ChatId != request.ChatId)
                return ServiceResult<ChatMessageDTO>.Failure(
                    "Отказано в доступе", HttpStatusCode.Forbidden
                );

            //закреплять сообщения можно кому угодно

            message.IsPinned = request.isPinned;
            message.PinnedAt = request.isPinned ? DateTime.UtcNow : null;

            await _dbContext.SaveChangesAsync();

            await _userService.GetUserById(message.SenderId);

            return ServiceResult<ChatMessageDTO>.Success(
                MapToDto(
                    message, 
                    (await _userService.GetUserById(message.SenderId)).Result,
                    message.SenderId == initiatorId
                )
            );
        }

        public async Task<ServiceResult<bool>> DeleteMessage(
            Guid initiatorId, DeleteChatMessageRequest request
        )
        {
            if (initiatorId == Guid.Empty)
                return ServiceResult<bool>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (request.ChatId == Guid.Empty)
                return ServiceResult<bool>.Failure("Id чата не может быть пустым");

            if (request.MessageId == Guid.Empty)
                return ServiceResult<bool>.Failure("Id сообщения не может быть пустым");

            var message = await _dbContext.ChatMessages
                .FirstOrDefaultAsync(cm => cm.Id == request.MessageId);

            // если сообщение не существует
            if (message is null)
                return ServiceResult<bool>.Failure(
                    "Сообщение не найдено", HttpStatusCode.NotFound
                );

            // если сообщение вообще в дургом чате
            if (message.ChatId != request.ChatId)
                return ServiceResult<bool>.Failure(
                    "Отказано в доступе", HttpStatusCode.Forbidden
                );

            ServiceResult<Chat> chat = await _chatService.GetChatBySystem(request.ChatId);
            if (!chat.IsSuccess || chat.Result is null)
                return ServiceResult<bool>.Failure(
                    ErrorCode.InternalServerError.GetDescription(), 
                    HttpStatusCode.InternalServerError
                );

            //в личный чатах можно удалять только тому кто отправил
            //в групповых чатах можно удалять только тому кто отправил и/или администратору и/или владельцу

            // если пытается удалить кто то другой
            if (message.SenderId != initiatorId)
            {
                //если это из личного чата - отказ
                if (chat.Result.Type == ChatType.Personal)
                    return ServiceResult<bool>.Failure("Отказано в доступе", HttpStatusCode.Forbidden);

                //если из группового
                // получаем того кто хочет удалить
                var initiatorMember = await _dbContext.ChatMembers
                    .FirstOrDefaultAsync(cm => cm.MemberId == initiatorId);

                if (initiatorMember is null
                    || (initiatorMember.Role != ChatMemberRole.Owner 
                        && initiatorMember.Role != ChatMemberRole.Administrator)
                )
                    return ServiceResult<bool>.Failure("Отказано в доступе", HttpStatusCode.Forbidden);
            }

            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }
    }
}
