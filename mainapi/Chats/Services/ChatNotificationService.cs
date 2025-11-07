using LunkvayAPI.Chats.Controllers;
using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LunkvayAPI.Chats.Services
{
    public class ChatNotificationService(
        IHubContext<ChatHub> hubContext,
        IWebSocketConnectionManager webSocketManager
    ) 
    : IChatNotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext = hubContext;
        private readonly IWebSocketConnectionManager _webSocketManager = webSocketManager;

        public async Task UpdateChat(Guid roomId, ChatDTO updatedChat)
        {
            await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("ChatUpdated", updatedChat);

            await _webSocketManager.SendToRoomAsync(
                roomId, new { Type = "ChatUpdated", Data = updatedChat }
            );
        }

        public async Task DeleteChat(Guid roomId, Guid chatId)
        {
            await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("ChatDeleted", chatId);

            await _webSocketManager.SendToRoomAsync(
                roomId, new { Type = "ChatDeleted", Data = chatId }
            );
        }

        // =======

        public async Task UpdateMember(Guid roomId, ChatMemberDTO updatedMember)
        {
            await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("MemberUpdated", updatedMember);

            await _webSocketManager.SendToRoomAsync(
                roomId, new { Type = "MemberUpdated", Data = updatedMember }
            );
        }

        public async Task DeleteMember(Guid roomId, Guid memberId)
        {
            await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("MemberDeleted", memberId);

            await _webSocketManager.SendToRoomAsync(
                roomId, new { Type = "MemberDeleted", Data = memberId }
            );
        }

        // =======

        public async Task SendMessage(Guid roomId, ChatMessageDTO message)
        {
            await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("ReceiveMessage", message);

            await _webSocketManager.SendToRoomAsync(
                roomId, new { Type = "ReceiveMessage", Data = message }
            );
        }

        public async Task UpdateMessage(Guid roomId, ChatMessageDTO updatedMessage)
        {
            await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("MessageUpdated", updatedMessage);

            await _webSocketManager.SendToRoomAsync(
                roomId, new { Type = "MessageUpdated", Data = updatedMessage }
            );
        }

        public async Task DeleteMessage(Guid roomId, Guid messageId)
        {
            await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("MessageDeleted", messageId);

            await _webSocketManager.SendToRoomAsync(
                roomId, new { Type = "MessageDeleted", Data = messageId }
            );
        }

        public async Task PinMessage(Guid roomId, Guid messageId, bool isPinned)
        {
            await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("MessagePinned", messageId, isPinned);

            await _webSocketManager.SendToRoomAsync(
                roomId, 
                new { 
                    Type = "MessagePinned", 
                    Data = new { MessageId = messageId, IsPinned = isPinned } 
                }
            );
        }
    }
}
