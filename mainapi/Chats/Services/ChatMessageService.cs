using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Enums.ErrorCodes;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Users.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace LunkvayAPI.Chats.Services
{
    public class ChatMessageService(
        ILogger<ChatMessageService> logger,
        LunkvayDBContext lunkvayDBContext,
        IUserSystemService userService,
        IChatSystemService chatService,
        IChatMemberSystemService chatMemberService,
        IChatNotificationService chatNotificationService
    ) : IChatMessageService
    {
        private readonly ILogger<ChatMessageService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly IUserSystemService _userService = userService;
        private readonly IChatSystemService _chatService = chatService;
        private readonly IChatMemberSystemService _chatMemberService = chatMemberService;
        private readonly IChatNotificationService _chatNotificationService = chatNotificationService;

        private async Task<ChatMessageDTO> MapToDto(
            ChatMessage message,
            Guid? senderid, string? userName, string? firstName, string? LastName, bool? isOnline, 
            bool isCurrentUser
        ) => new()
        {
            Id = message.Id,
            SenderId = senderid,
            SenderUserName = senderid is not null 
                ? (await _chatMemberService.GetChatMemberByChatIdAndMemberId(
                    message.ChatId, senderid.Value
                ))?.MemberName ?? userName
                : null,
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

        private async Task<ServiceResult<List<ChatMessageDTO>>> GetMessages(
            Guid userId, int page, int pageSize,
            Expression<Func<ChatMessage, bool>> predicate
        )
        {
            var chatMessages = await _dbContext.ChatMessages
                .Where(predicate)
                .OrderByDescending(cm => cm.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cm => new ChatMessageDTO
                {
                    Id = cm.Id,
                    SenderId = cm.SenderId,
                    SenderUserName = cm.Sender != null
                        ? (cm.Chat!.Members
                            .Where(m => m.MemberId == cm.SenderId && !m.IsDeleted)
                            .Select(m => m.MemberName)
                            .FirstOrDefault() ?? cm.Sender.UserName)
                        : null,
                    SenderFirstName = cm.Sender != null ? cm.Sender.FirstName : null,
                    SenderLastName = cm.Sender != null ? cm.Sender.LastName : null,
                    SenderIsOnline = cm.Sender != null && cm.Sender.IsOnline,
                    SystemMessageType = cm.SystemMessageType,
                    Message = cm.Message,
                    IsEdited = cm.IsEdited,
                    IsPinned = cm.IsPinned,
                    CreatedAt = cm.CreatedAt,
                    UpdatedAt = cm.UpdatedAt,
                    PinnedAt = cm.PinnedAt,
                    IsMyMessage = cm.SenderId == userId
                })
                .ToListAsync();

            _logger.LogInformation("({Date}) Получено {Count} сообщений", DateTime.UtcNow, chatMessages.Count);

            return ServiceResult<List<ChatMessageDTO>>.Success(chatMessages);
        }


        public async Task<ServiceResult<List<ChatMessageDTO>>> GetChatMessages(Guid userId, Guid chatId, int page, int pageSize)
        {
            _logger.LogInformation(
                "({Date}) Запрос списка сообщений для пользователя {UserId} в чате {ChatId}", 
                DateTime.Now, userId, chatId
            );

            return await GetMessages(
                userId, page, pageSize, 
                cm => cm.ChatId == chatId && !cm.IsDeleted
            );
        }

        public async Task<ServiceResult<List<ChatMessageDTO>>> GetPinnedChatMessages(Guid userId, Guid chatId, int page, int pageSize)
        {
            _logger.LogInformation("({Date}) Запрос списка сообщений для пользователя {UserId} в чате {ChatId}", DateTime.Now, userId, chatId);

            return await GetMessages(
                userId, page, pageSize, 
                cm => cm.ChatId == chatId && !cm.IsDeleted && cm.IsPinned
            );
        }

        public async Task<ServiceResult<ChatMessageDTO>> CreateMessage(Guid senderId, CreateChatMessageRequest request)
        {
            if (senderId == Guid.Empty)
                return ServiceResult<ChatMessageDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (string.IsNullOrWhiteSpace(request.Message))
                return ServiceResult<ChatMessageDTO>.Failure("Сообщение не может быть пустым");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                Guid? chatId = null;

                // если указан id существующего чата
                if (request.ChatId.HasValue && request.ChatId.Value != Guid.Empty)
                    chatId = request.ChatId.Value;
                // если сообщение НЕ в чат, а кому то КОНКРЕТНОМУ и оно НОВОЕ
                else if (request.ReceiverId.HasValue && request.ReceiverId.Value != Guid.Empty)
                {
                    if (request.ReceiverId.Value == senderId)
                        return ServiceResult<ChatMessageDTO>.Failure("Нельзя отправить сообщение самому себе");

                    var existingChatId 
                        = await _chatService.FindPersonalChatBetweenUsers(senderId, request.ReceiverId.Value);

                    if (existingChatId.HasValue)
                        chatId = existingChatId.Value;
                    else
                    {
                        var newChat = await _chatService.CreatePersonalChat(
                            ChatType.Personal, null
                        );
                        if (!newChat.IsSuccess || newChat.Result is null)
                            return ServiceResult<ChatMessageDTO>.Failure(
                                newChat.Error ?? "Не удалось создать чат",
                                HttpStatusCode.InternalServerError
                            );
                        chatId = newChat.Result.Id;
                        var newChatMembers = await _chatMemberService.CreatePersonalMembers(
                            chatId.Value, senderId, request.ReceiverId.Value
                        );
                        if (!newChatMembers.IsSuccess)
                            return ServiceResult<ChatMessageDTO>.Failure(
                                newChatMembers.Error ?? "Не удалось добавить участников в чат",
                                HttpStatusCode.InternalServerError
                            );
                    }
                }

                //если Id чата все еще null, то значит не переда Id чата
                if (!chatId.HasValue)
                    return ServiceResult<ChatMessageDTO>.Failure("Не указан получатель или чат");

                if (!await _chatMemberService.ExistChatMembers(chatId.Value, senderId))
                    return ServiceResult<ChatMessageDTO>.Failure("Вы не являетесь участником этого чата", HttpStatusCode.Forbidden);

                var chatMessage = new ChatMessage
                {
                    ChatId = chatId.Value,
                    SenderId = senderId,
                    SystemMessageType = SystemMessageType.None,
                    Message = request.Message
                };

                await _dbContext.AddAsync(chatMessage);
                await _dbContext.SaveChangesAsync();

                var chat = await _chatService.UpdateChatLastMessage(
                    chatId.Value, chatMessage.Id
                );

                var user = await _userService.GetUserById(senderId);
                if (user is null)
                    return ServiceResult<ChatMessageDTO>.Failure(
                        UsersErrorCode.UserNotFound.GetDescription(),
                        HttpStatusCode.NotFound
                    );

                var chatMessageDTO = await MapToDto(
                    chatMessage, 
                    user.Id, user.UserName, user.FirstName, user.LastName, user.IsOnline, 
                    chatMessage.SenderId == senderId
                );

                await _chatNotificationService.SendMessage(chatId.Value, chatMessageDTO);
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

            // если сообщение системное
            if (message.SystemMessageType != SystemMessageType.None)
                return ServiceResult<ChatMessageDTO>.Failure("Нельзя редактировать системные сообщения");

            message.Message = request.NewMessage.Trim();
            message.IsEdited = true;
            message.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var user = await _userService.GetUserById(message.SenderId.Value);
            if (user is null)
                return ServiceResult<ChatMessageDTO>.Failure(
                    UsersErrorCode.UserNotFound.GetDescription(),
                    HttpStatusCode.NotFound
                );

            var chatMessageDTO = await MapToDto(
                message,
                user.Id, user.UserName, user.FirstName, user.LastName, user.IsOnline,
                message.SenderId == editorId
            );

            await _chatNotificationService.UpdateMessage(request.ChatId, chatMessageDTO);

            return ServiceResult<ChatMessageDTO>.Success(chatMessageDTO);
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
            message.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            User? user = null;
            if (message.SenderId.HasValue)
                user = await _userService.GetUserById(message.SenderId.Value);

            await _chatNotificationService.PinMessage(
                request.ChatId, message.Id, request.isPinned, message.UpdatedAt
            );

            return ServiceResult<ChatMessageDTO>.Success(
                await MapToDto(
                    message, 
                    user?.Id, user?.UserName, user?.FirstName, user?.LastName, user?.IsOnline,
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

            ServiceResult<Chat> chat = await _chatService.GetChat(request.ChatId);
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

            await _chatNotificationService.DeleteMessage(request.ChatId, request.MessageId);

            return ServiceResult<bool>.Success(true);
        }
    }
}
