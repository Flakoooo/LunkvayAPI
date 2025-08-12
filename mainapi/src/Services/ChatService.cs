using LunkvayAPI.src.Controllers;
using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities.ChatAPI;
using LunkvayAPI.src.Models.Enums.ChatEnum;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.Interfaces;
using LunkvayAPI.src.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.src.Services
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
            List<ChatDTO> chats = await _dbContext.Chats
                .Where(c => c.Members.Any(m => m.MemberId == userId))
                .Select(c => new ChatDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    LastMessage = c.LastMessage != null ? new ChatMessageDTO
                    {
                        Message = c.LastMessage.Message,
                        CreatedAt = c.LastMessage.CreatedAt,
                        SystemMessageType = c.LastMessage.SystemMessageType,
                        Sender = c.LastMessage.Sender != null ? new UserDTO
                        {
                            UserName = c.LastMessage.Sender.UserName,
                            FirstName = c.LastMessage.Sender.FirstName,
                            LastName = c.LastMessage.Sender.LastName,
                        } : null
                    } : null
                })
                .ToListAsync();

            return ServiceResult<IEnumerable<ChatDTO>>.Success(chats);
        }

        public async Task<ServiceResult<ChatDTO>> CreateRoom(ChatRequest chatRequest, Guid? creatorId)
        {
            Chat chat = Chat.Create(creatorId, null, chatRequest.Name, null, chatRequest.Type);
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

            ChatMember chatMember = ChatMember.Create(roomId, newMemberId, null, ChatMemberRole.Member);
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

            Chat chat = await _dbContext.Chats.Where(c => c.Id == roomId).FirstAsync();
            ChatMember chatMember = ChatMember.Create(roomId, newMemberId, null, ChatMemberRole.Member);
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
