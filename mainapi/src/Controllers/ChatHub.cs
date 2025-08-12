using Microsoft.AspNetCore.SignalR;

namespace LunkvayAPI.src.Controllers
{
    public class ChatHub : Hub
    {
        // Подключение к конкретной комнате
        public async Task JoinRoom(Guid roomId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());

        // Отправка сообщения в конкретную комнату
        public async Task SendToRoom(Guid roomId, Guid userId, string message)
            => await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", userId, message);

        // Покинуть комнату
        public async Task LeaveRoom(Guid roomId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
    }
}
