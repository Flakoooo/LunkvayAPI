using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace LunkvayAPI.src.Controllers.ChatAPI
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, Guid> _userRooms = new();

        public async Task JoinRoom(Guid roomId)
        {
            _userRooms[Context.ConnectionId] = roomId;
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Caller.SendAsync("JoinedRoom", roomId);
        }

        public async Task SendToRoom(Guid roomId, Guid userId, string message)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", userId, message);

            Console.WriteLine($"Message sent to room {roomId} by user {userId}");
        }

        public async Task LeaveRoom(Guid roomId)
        {
            _userRooms.TryRemove(Context.ConnectionId, out _);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Caller.SendAsync("LeftRoom", roomId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_userRooms.TryRemove(Context.ConnectionId, out Guid roomId))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());

            await base.OnDisconnectedAsync(exception);
        }
    }
}
