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
        IUserService userService
    ) : IChatService
    {
        private readonly ILogger<ChatService> _logger = logger;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;
        private readonly IUserService _userService = userService;

        public async Task<ServiceResult<List<ChatDTO>>> GetRooms(Guid userId)
        {
            _logger.LogInformation("({Date}) Запрос списка чатов для {UserId}", DateTime.Now, userId);

            var chatQuery = _dbContext.Chats
                .Where(c => c.Members.Any(m => m.MemberId == userId))
                .Include(c => c.Members)
                    .ThenInclude(m => m.Member)
                .Include(c => c.LastMessage)
                .AsQueryable();

            var chats = await chatQuery.ToListAsync();

            foreach (var chat in chats.Where(c => c.LastMessage != null))
            {
                await _dbContext.Entry(chat.LastMessage!)
                    .Reference(m => m.Sender)
                    .LoadAsync();
            }

            var sortedChats = chats
                .OrderByDescending(c => c.LastMessage != null
                    ? c.LastMessage.CreatedAt
                    : c.UpdatedAt ?? c.CreatedAt)
                .ToList();

            List<ChatDTO> chatDtos = [.. sortedChats.Select(c => new ChatDTO
            {
                Id = c.Id,
                Name = c.Type == ChatType.Personal
                    ? c.Members
                        .Where(m => m.MemberId != userId)
                        .Select(m => m.Member?.FullName)
                        .FirstOrDefault()
                    : c.Name,
                LastMessage = c.LastMessage != null ? new ChatMessageDTO
                {
                    Message = c.LastMessage.Message,
                    CreatedAt = c.LastMessage.CreatedAt,
                    SystemMessageType = c.LastMessage.SystemMessageType,
                    Sender = c.LastMessage.Sender != null ? new UserDTO
                    {
                        Id = c.LastMessage.Sender.Id,
                        UserName = c.LastMessage.Sender.UserName,
                        FirstName = c.LastMessage.Sender.Id != userId
                            ? c.Type != ChatType.Personal
                                ? c.LastMessage.Sender.FirstName
                                : null
                            : "Вы",
                        LastName = c.Type != ChatType.Personal
                            ? !string.IsNullOrEmpty(c.LastMessage.Sender.LastName)
                                ? c.LastMessage.Sender.LastName[0].ToString()
                                : null
                            : null
                    } : null
                } : null
            })];

            _logger.LogInformation("({Date}) Получено {Count} чатов", DateTime.UtcNow, chatDtos.Count);

            return ServiceResult<List<ChatDTO>>.Success(chatDtos);
        }

        public async Task<ServiceResult<ChatDTO>> CreateRoom(ChatRequest chatRequest, Guid? creatorId)
        {
            Chat chat = new()
            {
                CreatorId = creatorId, 
                LastMessageId = null, 
                Name = chatRequest.Name,
                Type = chatRequest.Type
            };
            await _dbContext.AddAsync(chat);
            await _dbContext.SaveChangesAsync();

            ChatDTO chatDTO = new()
            {
                Id = chat.Id,
                LastMessage = null,
                Name = chat.Name,
            };

            foreach (UserDTO member in chatRequest.Members)
            {
                if (member.Id != Guid.Empty)
                {
                    ChatMember chatMember = new()
                    {
                        ChatId = chat.Id,
                        MemberId = member.Id,
                        Role = ChatMemberRole.Member
                    };
                    await _dbContext.AddAsync(chatMember);
                }
            }

            await _dbContext.SaveChangesAsync();

            // сделать уведомление в чате всех участников что чат создан

            return ServiceResult<ChatDTO>.Success(chatDTO);
        }

        public async Task<ServiceResult<ChatDTO>> UpdateChat(
            Guid creatorId, Guid chatId, UpdateChatRequest request
        )
        {
            if (creatorId == Guid.Empty)
                return ServiceResult<ChatDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (chatId == Guid.Empty)
                return ServiceResult<ChatDTO>.Failure("Id чата не может быть пустым");

            var chat = await _dbContext.Chats
                .FirstOrDefaultAsync(c => c.CreatorId == creatorId && c.Id == chatId);

            if (chat is null)
                return ServiceResult<ChatDTO>.Failure("Не удалось обновить чат");

            if (chat.Type == ChatType.Personal)
                return ServiceResult<ChatDTO>.Failure("Данный чат невозможно изменить");

            bool hasChanges = false;
            if (request.Name is not null)
            {
                chat.Name = request.Name;
                hasChanges = true;
            }

            if (hasChanges) await _dbContext.SaveChangesAsync();

            //сделать уведомление в чате что чат обновлен

            var chatDTO = new ChatDTO
            {
                Id = chat.Id,
                Name = chat.Name,
            };

            var chatMessage = await _dbContext.ChatMessages
                .LastOrDefaultAsync(cm => cm.ChatId == chat.Id);

            if (chatMessage is not null)
            {
                ServiceResult<UserDTO> userResult
                    = await _userService.GetUserById(chatMessage.SenderId);

                if (!userResult.IsSuccess || userResult.Result is null)
                    return ServiceResult<ChatDTO>.Failure(
                        ErrorCode.InternalServerError.GetDescription()
                    );

                var user = userResult.Result;
                bool currentUserCheck = user.Id == creatorId;
                user.FirstName = currentUserCheck ? "Вы" : user.FirstName;
                user.LastName = !string.IsNullOrEmpty(user.LastName)
                    ? currentUserCheck ? null : user.LastName[0].ToString()
                    : null;

                var lastMessage = new ChatMessageDTO
                {
                    Id = chatMessage.Id,
                    Sender = user,
                    SystemMessageType = SystemMessageType.None,
                    Message = chatMessage.Message,
                    CreatedAt = chatMessage.CreatedAt
                };
            }

            return ServiceResult<ChatDTO>.Success(chatDTO);
        }

        public async Task<ServiceResult<bool>> DeleteChat(Guid creatorId, Guid chatId)
        {
            if (creatorId == Guid.Empty)
                return ServiceResult<bool>.Failure(ErrorCode.UserIdRequired.GetDescription());

            if (chatId == Guid.Empty)
                return ServiceResult<bool>.Failure("Id чата не может быть пустым");

            var chat = await _dbContext.Chats
                .FirstOrDefaultAsync(c => c.CreatorId == creatorId && c.Id == chatId);

            if (chat is null)
                return ServiceResult<bool>.Failure("Не удалось удалить чат");

            chat.IsDeleted = true;
            chat.DeletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }
    }
}
