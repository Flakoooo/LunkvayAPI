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

namespace LunkvayAPI.Chats.Services
{
    public class ChatService(
        ILogger<ChatService> logger, 
        LunkvayDBContext lunkvayDBContext,
        IUserService userService,
        IChatMemberService chatMemberService,
        IChatMessageService chatMessageService
    ) : IChatService
    {
        private readonly ILogger<ChatService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly IUserService _userService = userService;
        private readonly IChatMemberService _chatMemberService = chatMemberService;
        private readonly IChatMessageService _chatMessageService = chatMessageService;

        private static ChatDTO MapToChatDto(Chat chat, Guid currentUserId)
            => new()
            {
                Id = chat.Id,
                Name = GetChatName(chat, currentUserId),
                Type = chat.Type,
                LastMessage = chat.LastMessage != null 
                    ? MapToMessageDto(chat.LastMessage, chat.Type, currentUserId) 
                    : null,
                CreatedAt = chat.CreatedAt,
                MemberCount = chat.Members.Count
            };

        private static string? GetChatName(Chat chat, Guid currentUserId)
            => chat.Type == ChatType.Personal
                ? chat.Members
                    .Where(m => m.MemberId != currentUserId)
                    .Select(m => m.Member?.FullName)
                    .FirstOrDefault()
                : chat.Name;

        private static ChatMessageDTO? MapToMessageDto(ChatMessage message, ChatType chatType, Guid currentUserId)
            => message != null ? new ChatMessageDTO
            {
                Id = message.Id,
                Message = message.Message,
                CreatedAt = message.CreatedAt,
                SystemMessageType = message.SystemMessageType,
                Sender = message.Sender != null 
                    ? MapToUserDto(message.Sender, chatType, currentUserId) 
                    : null
            } : null;

        private static UserDTO MapToUserDto(User user, ChatType chatType, Guid currentUserId)
            => new()
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.Id == currentUserId
                    ? "Вы"
                    : (chatType != ChatType.Personal 
                        ? user.FirstName 
                        : null
                    ),
                LastName = chatType != ChatType.Personal && !(user.Id == currentUserId)
                    ? (!string.IsNullOrEmpty(user.LastName) 
                        ? user.LastName[0].ToString() 
                        : null
                    )
                    : null
            };

        private static string FormatUserName(UserDTO? user)
        {
            if (user == null) return "Пользователь";

            if (!string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName))
                return $"{user.FirstName} {user.LastName}";

            if (!string.IsNullOrWhiteSpace(user.FirstName))
                return user.FirstName;

            if (!string.IsNullOrWhiteSpace(user.LastName))
                return user.LastName;

            return user.UserName ?? "Пользователь";
        }

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



        public async Task<ServiceResult<List<ChatDTO>>> GetRooms(Guid userId)
        {
            _logger.LogInformation("({Date}) Запрос списка чатов для {UserId}", DateTime.Now, userId);

            var chats = await _dbContext.Chats
                .Where(c => c.Members.Any(m => m.MemberId == userId) && !c.IsDeleted)
                .Include(c => c.Members)
                    .ThenInclude(m => m.Member)
                .Include(c => c.LastMessage)
                    .ThenInclude(m => m!.Sender)
                .OrderByDescending(c => c.LastMessage != null
                    ? c.LastMessage.CreatedAt
                    : c.UpdatedAt ?? c.CreatedAt)
                .ToListAsync();

            var chatDtos = chats.Select(c => MapToChatDto(c, userId)).ToList();

            _logger.LogInformation("({Date}) Получено {Count} чатов", DateTime.UtcNow, chatDtos.Count);
            return ServiceResult<List<ChatDTO>>.Success(chatDtos);
        }

        public async Task<ServiceResult<ChatDTO>> WriteAndCreatePersonalChat(Guid creatorId, CreatePersonalChatRequest request)
        {
            if (creatorId == Guid.Empty || request.Interlocutor.Id == Guid.Empty)
                return ServiceResult<ChatDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (string.IsNullOrWhiteSpace(request.Message))
                return ServiceResult<ChatDTO>.Failure("Первое сообщение личного чата не может быть пустым");

            if (creatorId == request.Interlocutor.Id)
                return ServiceResult<ChatDTO>.Failure("Нельзя создать личный чат с самим собой");


            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var chat = await _dbContext.Chats
                    .Where(c => c.Type == ChatType.Personal && !c.IsDeleted)
                    .Where(c => c.Members.Any(m => m.MemberId == creatorId) &&
                               c.Members.Any(m => m.MemberId == request.Interlocutor.Id))
                    .FirstOrDefaultAsync();

                if (chat is null)
                {
                    chat = new Chat { Type = ChatType.Personal };

                    await _dbContext.Chats.AddAsync(chat);
                    await _dbContext.SaveChangesAsync();

                    var result = await _chatMemberService.CreatePersonalMembersBySystem(
                        chat.Id, creatorId, request.Interlocutor.Id
                    );
                    if (!result.IsSuccess)
                        throw new Exception(
                            result.Error ?? ErrorCode.InternalServerError.GetDescription()
                        );
                }

                var message = new ChatMessage
                {
                    ChatId = chat.Id,
                    SenderId = creatorId,
                    Message = request.Message,
                    SystemMessageType = SystemMessageType.None,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.ChatMessages.AddAsync(message);
                chat.LastMessageId = message.Id;
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                var fullChat = await _dbContext.Chats
                    .Include(c => c.Members)
                        .ThenInclude(m => m.Member)
                    .Include(c => c.LastMessage)
                        .ThenInclude(m => m!.Sender)
                    .FirstOrDefaultAsync(c => c.Id == chat.Id);

                var chatDto = MapToChatDto(fullChat!, creatorId);
                return ServiceResult<ChatDTO>.Success(chatDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при создании чата");
                return ServiceResult<ChatDTO>.Failure("Ошибка при создании чата");
            }
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

                var result =  await _chatMemberService.CreateGroupMembersBySystem(
                    chat.Id, creatorId, chatRequest.Members
                );
                if (!result.IsSuccess)
                    throw new Exception(
                        result.Error ?? ErrorCode.InternalServerError.GetDescription()
                    );

                ServiceResult<UserDTO> userResult = await _userService.GetUserById(creatorId);
                var user = userResult.Result;

                ServiceResult<ChatMessage> messageResult = await _chatMessageService.CreateSystemChatMessage(
                    chat.Id, $"{FormatUserName(user)} создал чат {chat.Name}", SystemMessageType.ChatCreated
                );

                chat.LastMessageId = messageResult.Result?.Id;
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                var chatDto = MapToChatDto(chat, creatorId);
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

            var chatDto = MapToChatDto(chat, userId);
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

            return ServiceResult<bool>.Success(true);
        }
    }
}
