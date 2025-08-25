using LunkvayAPI.src.Controllers.ChatAPI;
using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities.ChatAPI;
using LunkvayAPI.src.Models.Enums.ChatEnum;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.ChatAPI.Interfaces;
using LunkvayAPI.src.Services.Interfaces;
using LunkvayAPI.src.Utils;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;

namespace LunkvayAPI.src.Services.ChatAPI
{
    public class ChatService(
        ILogger<ChatService> logger, 
        IHubContext<ChatHub> hubContext, 
        IUserService userService,
        LunkvayDBContext lunkvayDBContext
    ) : IChatService
    {
        private readonly ILogger<ChatService> _logger = logger;
        private readonly IHubContext<ChatHub> _hubContext = hubContext;
        private readonly IUserService _userService = userService;
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        public async Task<ServiceResult<IEnumerable<ChatDTO>>> GetRooms(Guid userId)
        {
            _logger.LogInformation("({Date}) Запрос списка чатов для {UserId}", DateTime.Now, userId);

            List<ChatDTO> chats = await _dbContext.Chats
                .Where(c => c.Members.Any(m => m.MemberId == userId))
                .Select(c => new ChatDTO
                {
                    Id = c.Id,
                    Name = c.Type == ChatType.Personal
                        ? c.Members
                            .Where(m => m.MemberId != userId)
                            .Select(m => m.Member != null ? m.Member.FullName : null)
                            .FirstOrDefault()
                        : c.Name,
                    LastMessage = c.LastMessage != null ? new ChatMessageDTO
                    {
                        Message = c.LastMessage.Message,
                        CreatedAt = c.LastMessage.CreatedAt,
                        SystemMessageType = c.LastMessage.SystemMessageType,
                        Sender = c.LastMessage.Sender != null ? new UserDTO
                        {
                            UserName = c.LastMessage.Sender.UserName,
                            FirstName = c.LastMessage.Sender.Id != userId
                                ? c.Type != ChatType.Personal
                                    ? c.LastMessage.Sender.FirstName
                                    : null
                                : "Вы",
                            LastName = c.Type != ChatType.Personal
                                ? c.LastMessage.Sender.LastName != null
                                    ? c.LastMessage.Sender.LastName.Length > 0
                                        ? c.LastMessage.Sender.LastName[0].ToString()
                                        : null
                                    : null
                                : null
                        } : null
                    } : null
                })
                .ToListAsync();

            _logger.LogInformation("({Date}) Получено {Count} чатов", DateTime.UtcNow, chats.Count);

            return ServiceResult<IEnumerable<ChatDTO>>.Success(chats);
        }

        public async Task<ServiceResult<IEnumerable<ChatMessageDTO>>> GetChatMessages(Guid userId, Guid chatId, int page, int pageSize)
        {
            _logger.LogInformation("({Date}) Запрос списка сообщений для пользователя {UserId} в чате {ChatId}", DateTime.Now, userId, chatId);

            List<ChatMessageDTO> chatMessages = await _dbContext.ChatMessages
                .Where(cm => cm.ChatId == chatId)
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
                if (member.Id is not null)
                {
                    ChatMember chatMember = new()
                    {
                        ChatId = chat.Id,
                        MemberId = (Guid)member.Id,
                        Role = ChatMemberRole.Member
                    };
                    await _dbContext.AddAsync(chatMember);
                }
            }

            await _dbContext.SaveChangesAsync();

            return ServiceResult<ChatDTO>.Success(chatDTO);
        }

        public async Task InviteInRoom(Guid roomId, Guid senderId, Guid newMemberId)
        {
            ServiceResult<UserDTO> sender = await _userService.GetUserById(senderId);
            string senderName = sender.IsSuccess && sender.Result is not null
                ? sender.Result.FirstName is not null && sender.Result.LastName is not null
                    ? $"{sender.Result.FirstName} {sender.Result.LastName}"
                    : $"{sender.Result.UserName}"
                : "[ОШИБКА]";

            ServiceResult<UserDTO> newMember = await _userService.GetUserById(newMemberId);
            string newbeName = newMember.IsSuccess && newMember.Result is not null
                ? newMember.Result.FirstName is not null && newMember.Result.LastName is not null
                    ? $"{newMember.Result.FirstName} {newMember.Result.LastName}"
                    : $"{newMember.Result.UserName}"
                : "[ОШИБКА]";

            ChatMember chatMember = new()
            {
                ChatId = roomId,
                MemberId = newMemberId,
                MemberName = null,
                Role = ChatMemberRole.Member
            };
            await _dbContext.ChatMembers.AddAsync(chatMember);

            await _hubContext.Clients.Group(roomId.ToString()).SendAsync("Notify", $"{senderName} пригласил {newbeName} в чат");

            await _dbContext.SaveChangesAsync();
        }

        public async Task JoinInRoom(Guid roomId, Guid newMemberId)
        {
            ServiceResult<UserDTO> newMember = await _userService.GetUserById(newMemberId);
            string newbeName = newMember.IsSuccess && newMember.Result is not null
                ? newMember.Result.FirstName is not null && newMember.Result.LastName is not null
                    ? $"{newMember.Result.FirstName} {newMember.Result.LastName}"
                    : $"{newMember.Result.UserName}"
                : "[ОШИБКА]";

            Chat chat = await _dbContext.Chats.FirstAsync(c => c.Id == roomId);
            ChatMember chatMember = new()
            {
                ChatId = roomId,
                MemberId = newMemberId,
                MemberName = null,
                Role = ChatMemberRole.Member
            };
            await _dbContext.ChatMembers.AddAsync(chatMember);

            if (chat.Type is ChatType.Group)
                await _hubContext.Clients.Group(roomId.ToString()).SendAsync("Notify", $"{newbeName} вошел в чат");

            await _dbContext.SaveChangesAsync();
        }

        public async Task LeaveFromRoom(Guid roomId, Guid userId)
        {
            ServiceResult<UserDTO> user = await _userService.GetUserById(userId);
            string leftName = user.IsSuccess && user.Result is not null
                ? user.Result.FirstName is not null && user.Result.LastName is not null
                    ? $"{user.Result.FirstName} {user.Result.LastName}"
                    : $"{user.Result.UserName}"
                : "[ОШИБКА]";

            await _dbContext.ChatMembers
                .Where(c => c.ChatId == roomId && c.MemberId == userId)
                .ExecuteDeleteAsync();

            await _hubContext.Clients.Group(roomId.ToString()).SendAsync("Notify", $"{leftName} покинул чат");

            await _dbContext.SaveChangesAsync();
        }
    }
}
