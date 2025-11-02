using LunkvayAPI.Chats.Controllers;
using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LunkvayAPI.Chats.Services
{
    public class ChatNotificationService(IHubContext<ChatHub> hubContext) : IChatNotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext = hubContext;

        public async Task SendMessage(Guid roomId, ChatMessageDTO message)
            => await _hubContext.Clients.Group(roomId.ToString())
            .SendAsync("ReceiveMessage", message);
    }
}
