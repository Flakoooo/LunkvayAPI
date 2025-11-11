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
using System.Net;

namespace LunkvayAPI.Chats.Services
{
    public class ChatService(
        ILogger<ChatService> logger, 
        LunkvayDBContext lunkvayDBContext,
        IUserSystemService userService,
        IChatMemberSystemService chatMemberService,
        IChatMessageSystemService chatMessageService,
        IChatNotificationService chatNotificationService
    ) : IChatService
    {
        private readonly ILogger<ChatService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly IUserSystemService _userService = userService;
        private readonly IChatMemberSystemService _chatMemberService = chatMemberService;
        private readonly IChatMessageSystemService _chatMessageService = chatMessageService;
        private readonly IChatNotificationService _chatNotificationService = chatNotificationService;

        private static ChatDTO MapToChatDto(
            Chat chat, Guid currentUserId, Dictionary<(Guid, Guid), string?> memberNamesDict
        ) => new()
        {
            Id = chat.Id,
            Name = GetChatName(chat, currentUserId),
            Type = chat.Type,
            LastMessage = MapToMessageDto(chat.LastMessage, chat.Type, currentUserId, memberNamesDict),
            CreatedAt = chat.CreatedAt,
            MemberCount = chat.Members.Count
        };

        private static string? GetChatName(Chat chat, Guid currentUserId)
            => chat.Type == ChatType.Personal
                ? chat.Members
                    .Where(m => m.MemberId != currentUserId)
                    .Select(m => m.Member?.FullName ?? m.Member?.UserName) 
                    .FirstOrDefault() ?? "Чат" 
                : chat.Name;

        private static ChatMessageDTO? MapToMessageDto(
            ChatMessage? message, ChatType chatType, Guid currentUserId,
            Dictionary<(Guid, Guid), string?> memberNamesDict
        )
        {
            if (message == null) return null;

            string? displayName = null;
            if (message.SenderId.HasValue)
            {
                if (chatType == ChatType.Group)
                {
                    var key = (message.ChatId, message.SenderId.Value);
                    displayName = memberNamesDict.TryGetValue(key, out var customName)
                        ? customName
                        : message.Sender?.UserName;
                }
                else
                    displayName = message.Sender?.UserName;
            }

            return new ChatMessageDTO
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderUserName = displayName,
                SenderFirstName = message.SenderId == currentUserId
                    ? "Вы"
                    : (chatType != ChatType.Personal
                        ? message.Sender?.FirstName
                        : null
                    ),
                SenderLastName = chatType != ChatType.Personal && !(message.SenderId == currentUserId)
                    ? (!string.IsNullOrEmpty(message.Sender?.LastName)
                        ? message.Sender?.LastName[0].ToString()
                        : null
                    )
                    : null,
                SenderIsOnline = false,
                SystemMessageType = message.SystemMessageType,
                Message = message.Message,
                CreatedAt = message.CreatedAt,
                UpdatedAt = message.UpdatedAt,
                PinnedAt = message.PinnedAt,
                IsMyMessage = message.SenderId == currentUserId
            };
        }

        private static string FormatUserName(User user)
        {
            if (!string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName))
                return $"{user.FirstName} {user.LastName}";

            if (!string.IsNullOrWhiteSpace(user.FirstName))
                return user.FirstName;

            if (!string.IsNullOrWhiteSpace(user.LastName))
                return user.LastName;

            return user.UserName ?? "Пользователь";
        }



        public async Task<ServiceResult<List<ChatDTO>>> GetRooms(Guid userId)
        {
            _logger.LogInformation("({Date}) Запрос списка чатов для {UserId}", DateTime.Now, userId);

            var chats = await _dbContext.Chats
                .Where(c => c.Members.Any(m => m.MemberId == userId && !m.IsDeleted) && !c.IsDeleted)
                .Include(c => c.Members.Where(m => !m.IsDeleted))
                    .ThenInclude(m => m.Member)
                .Include(c => c.LastMessage)
                    .ThenInclude(m => m!.Sender)
                .OrderByDescending(c => c.LastMessage != null
                    ? c.LastMessage.CreatedAt
                    : c.UpdatedAt ?? c.CreatedAt)
                .ToListAsync();

            var chatIds = chats.Select(c => c.Id).Distinct().ToList();
            var senderIds = chats.Where(c => c.LastMessage?.SenderId != null)
                                .Select(c => c.LastMessage!.SenderId!.Value)
                                .Distinct()
                                .ToList();

            var memberNamesDict = new Dictionary<(Guid, Guid), string?>();
            foreach (var chatId in chatIds)
            {
                var members = await _chatMemberService.GetChatMembersByChatId(chatId);
                foreach (var member in members.Where(m => senderIds.Contains(m.MemberId)))
                {
                    memberNamesDict[(chatId, member.MemberId)] = member.MemberName;
                }
            }

            // Синхронный маппинг с использованием словаря
            var chatDtos = chats.Select(c => MapToChatDto(c, userId, memberNamesDict)).ToList();

            _logger.LogInformation("({Date}) Получено {Count} чатов", DateTime.UtcNow, chatDtos.Count);
            return ServiceResult<List<ChatDTO>>.Success(chatDtos);
        }

        public async Task<ServiceResult<ChatDTO>> CreateGroupChat(Guid creatorId, CreateGroupChatRequest chatRequest)
        {
            if (creatorId == Guid.Empty)
                return ServiceResult<ChatDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (string.IsNullOrWhiteSpace(chatRequest.Name))
                return ServiceResult<ChatDTO>.Failure("Название группового чата обязательно");

            if (!chatRequest.Members.Any())
                return ServiceResult<ChatDTO>.Failure("Чат должен содержать участников");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var chat = new Chat
                {
                    CreatorId = creatorId,
                    Name = chatRequest.Name.Trim(),
                    Type = ChatType.Group
                };

                await _dbContext.Chats.AddAsync(chat);
                await _dbContext.SaveChangesAsync();

                ServiceResult<List<ChatMember>> result =  await _chatMemberService.CreateGroupMembers(
                    chat.Id, creatorId, chatRequest.Members
                );
                if (!result.IsSuccess)
                    return ServiceResult<ChatDTO>.Failure(
                        result.Error ?? ErrorCode.InternalServerError.GetDescription(),
                        result.Error is null ? HttpStatusCode.InternalServerError : result.StatusCode
                    );

                var user = await _userService.GetUserById(creatorId);
                if (user == null)
                    return ServiceResult<ChatDTO>.Failure(
                        UsersErrorCode.UserNotFound.GetDescription(),
                        HttpStatusCode.NotFound
                    );

                ServiceResult<ChatMessage> messageResult = await _chatMessageService.CreateSystemChatMessage(
                    chat.Id, $"{FormatUserName(user)} создал чат {chat.Name}", SystemMessageType.ChatCreated
                );

                chat.LastMessageId = messageResult.Result?.Id;
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                var chatDto = MapToChatDto(chat, creatorId, []);
                return ServiceResult<ChatDTO>.Success(chatDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при создании чата");
                return ServiceResult<ChatDTO>.Failure("Ошибка при создании чата");
            }
        }

        public async Task<ServiceResult<ChatDTO>> UpdateChat(Guid userId, Guid chatId, UpdateChatRequest request)
        {
            if (userId == Guid.Empty)
                return ServiceResult<ChatDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (chatId == Guid.Empty)
                return ServiceResult<ChatDTO>.Failure("Id чата не может быть пустым");

            var userMembership = await _dbContext.ChatMembers
                .FirstOrDefaultAsync(cm => 
                    cm.ChatId == chatId 
                    && cm.MemberId == userId 
                    && (cm.Role == ChatMemberRole.Owner 
                        || cm.Role == ChatMemberRole.Administrator)
                    );

            if (userMembership == null)
                return ServiceResult<ChatDTO>.Failure("Недостаточно прав для редактирования чата");

            var chat = await _dbContext.Chats
                .Include(c => c.Members)
                .ThenInclude(m => m.Member)
                .FirstOrDefaultAsync(c => c.Id == chatId && !c.IsDeleted);

            if (chat is null)
                return ServiceResult<ChatDTO>.Failure("Чат не найден");

            if (chat.Type == ChatType.Personal)
                return ServiceResult<ChatDTO>.Failure("Личный чат невозможно изменить");

            bool hasChanges = false;

            if (request.NewName != null && request.NewName.Trim() != chat.Name)
            {
                var oldName = chat.Name;
                chat.Name = request.NewName.Trim();
                hasChanges = true;

                var systemMessage = new ChatMessage
                {
                    ChatId = chat.Id,
                    Message = $"Название чата изменено с {oldName} на {chat.Name}",
                    SystemMessageType = SystemMessageType.ChatUpdated,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.ChatMessages.AddAsync(systemMessage);
                chat.LastMessageId = systemMessage.Id;
            }

            if (hasChanges)
            {
                chat.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            var chatDto = MapToChatDto(chat, userId, []);

            await _chatNotificationService.UpdateChat(chatId, chatDto);

            return ServiceResult<ChatDTO>.Success(chatDto);
        }

        public async Task<ServiceResult<bool>> DeleteChat(Guid userId, Guid chatId)
        {
            if (userId == Guid.Empty)
                return ServiceResult<bool>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (chatId == Guid.Empty)
                return ServiceResult<bool>.Failure("Id чата не может быть пустым");

            var chat = await _dbContext.Chats
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == chatId && !c.IsDeleted);

            if (chat is null)
                return ServiceResult<bool>.Failure("Чат не найден");

            if (chat.Type == ChatType.Group)
            {
                if (chat.CreatorId != userId)
                    return ServiceResult<bool>.Failure("Недостаточно прав для удаления чата");
            }
            else return ServiceResult<bool>.Failure("Персональный чат удалить невозможно");

            chat.IsDeleted = true;
            chat.DeletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            await _chatNotificationService.DeleteChat(chatId, chatId);

            return ServiceResult<bool>.Success(true);
        }
    }
}
