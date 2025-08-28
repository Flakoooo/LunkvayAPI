using LunkvayAPI.src.Controllers.ChatAPI;
using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Services.ChatAPI.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LunkvayAPI.src.Services.ChatAPI
{
    public class ChatNotificationService(IHubContext<ChatHub> hubContext) : IChatNotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext = hubContext;

        public async Task UserInvited(Guid roomId, string inviterName, string newMemberName)
            => await _hubContext.Clients.Group(roomId.ToString())
            .SendAsync("Notify", $"{inviterName} пригласил {newMemberName} в чат");

        public async Task UserJoined(Guid roomId, string userName)
            => await _hubContext.Clients.Group(roomId.ToString())
            .SendAsync("Notify", $"{userName} присоединился к чату");

        public async Task UserLeft(Guid roomId, string userName)
            => await _hubContext.Clients.Group(roomId.ToString())
            .SendAsync("Notify", $"{userName} покинул чат");

        public async Task SendMessage(Guid roomId, ChatMessageDTO message)
            => await _hubContext.Clients.Group(roomId.ToString())
            .SendAsync("ReceiveMessage", message);
    }
}
