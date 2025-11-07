using System.Net.WebSockets;

namespace LunkvayAPI.Common.Interfaces
{
    public interface IWebSocketConnectionManager
    {
        void AddConnection(Guid roomId, WebSocket webSocket, string connectionId);
        void RemoveConnection(string connectionId);
        Task SendToRoomAsync(Guid roomId, object message);
        Task SendToAllAsync(object message);
    }
}
