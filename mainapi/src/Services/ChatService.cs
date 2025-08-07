using LunkvayAPI.src.Controllers;
using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Entities.ChatAPI;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.Interfaces;
using LunkvayApp.src.Models.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace LunkvayAPI.src.Services
{
    public class ChatService(
        ILogger<ChatService> logger, 
        IHubContext<ChatHub> hubContext, 
        UserService userService
    ) : IChatService
    {
        private readonly ILogger<ChatService> _logger = logger;
        private readonly IHubContext<ChatHub> _hubContext = hubContext;
        private readonly UserService _userService = userService;

        public Task<ServiceResult<Chat>> CreateRoom(string name, ChatType type)
        {
            throw new NotImplementedException();
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

            //создание участника чата
            ChatMember chatMember = new();

            await _hubContext.Clients.Group(roomId.ToString()).SendAsync("Notify", $"{senderName} пригласил {newbeName} в чат");
        }

        public async Task JoinInRoom(Guid roomId, Guid newMemberId)
        {
            ServiceResult<UserDTO> newMember = await _userService.GetUserById(newMemberId);
            string newbeName = newMember.IsSuccess && newMember.Result is not null
                ? newMember.Result.FirstName is not null && newMember.Result.LastName is not null
                    ? $"{newMember.Result.FirstName} {newMember.Result.LastName}"
                    : $"{newMember.Result.UserName}"
                : "[ОШИБКА]";

            //получение chatId
            Chat chat = new();

            //создание участника чата
            ChatMember chatMember = new();

            if (chat.Type is ChatType.Group)
            {
                await _hubContext.Clients.Group(roomId.ToString()).SendAsync("Notify", $"{newbeName} вошел в чат");
            }
        }

        public async Task LeaveFromRoom(Guid roomId, Guid userId)
        {
            ServiceResult<UserDTO> user = await _userService.GetUserById(userId);
            string leftName = user.IsSuccess && user.Result is not null
                ? user.Result.FirstName is not null && user.Result.LastName is not null
                    ? $"{user.Result.FirstName} {user.Result.LastName}"
                    : $"{user.Result.UserName}"
                : "[ОШИБКА]";

            await _hubContext.Clients.Group(roomId.ToString()).SendAsync("Notify", $"{leftName} покинул чат");
        }
    }
}
