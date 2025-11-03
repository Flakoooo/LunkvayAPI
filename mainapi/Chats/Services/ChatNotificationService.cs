using LunkvayAPI.Chats.Controllers;
using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LunkvayAPI.Chats.Services
{
    public class ChatNotificationService(IHubContext<ChatHub> hubContext) : IChatNotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext = hubContext;

        public async Task UpdateChat(Guid roomId, ChatDTO updatedChat)
            => await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("ChatUpdated", updatedChat);

        public async Task DeleteChat(Guid roomId, Guid chatId)
            => await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("ChatDeleted", chatId);

        // =======

        public async Task UpdateMember(Guid roomId, ChatMemberDTO updatedMember)
            => await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("MemberUpdated", updatedMember);

        public async Task DeleteMember(Guid roomId, Guid memberId)
            => await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("MemberDeleted", memberId);

        // =======

        public async Task SendMessage(Guid roomId, ChatMessageDTO message)
            => await _hubContext.Clients.Group(roomId.ToString())
            .SendAsync("ReceiveMessage", message);

        public async Task UpdateMessage(Guid roomId, ChatMessageDTO updatedMessage)
            => await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("MessageUpdated", updatedMessage);

        public async Task DeleteMessage(Guid roomId, Guid messageId)
            => await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("MessageDeleted", messageId);

        public async Task PinMessage(Guid roomId, Guid messageId, bool isPinned)
            => await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("MessagePinned", messageId, isPinned);
    }
}
