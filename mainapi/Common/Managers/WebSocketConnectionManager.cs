using LunkvayAPI.Common.Interfaces;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace LunkvayAPI.Common.Managers
{
    public class WebSocketConnectionManager : IWebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, WebSocket>> _roomConnections = new();
        private readonly ConcurrentDictionary<string, Guid> _connectionRooms = new();

        public void AddConnection(Guid roomId, WebSocket webSocket, string connectionId)
        {
            var roomConnections = _roomConnections.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, WebSocket>());
            roomConnections[connectionId] = webSocket;
            _connectionRooms[connectionId] = roomId;

            Console.WriteLine($"Connection {connectionId} added to room {roomId}");
        }

        public void RemoveConnection(string connectionId)
        {
            if (_connectionRooms.TryRemove(connectionId, out Guid roomId))
            {
                if (_roomConnections.TryGetValue(roomId, out var roomConnections))
                {
                    roomConnections.TryRemove(connectionId, out _);

                    if (roomConnections.IsEmpty)
                    {
                        _roomConnections.TryRemove(roomId, out _);
                    }
                }
            }

            Console.WriteLine($"Connection {connectionId} removed");
        }

        public async Task SendToRoomAsync(Guid roomId, object message)
        {
            if (_roomConnections.TryGetValue(roomId, out var connections))
            {
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                var segment = new ArraySegment<byte>(bytes);

                var tasks = connections.Values
                    .Where(ws => ws.State == WebSocketState.Open)
                    .Select(ws => ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None))
                    .ToArray();

                await Task.WhenAll(tasks);
                Console.WriteLine($"Message sent to room {roomId}, connections: {tasks.Length}");
            }
            else
            {
                Console.WriteLine($"No connections in room {roomId}");
            }
        }

        public async Task SendToAllAsync(object message)
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(bytes);

            var tasks = _roomConnections.Values
                .SelectMany(room => room.Values)
                .Where(ws => ws.State == WebSocketState.Open)
                .Select(ws => ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None))
                .ToArray();

            await Task.WhenAll(tasks);
            Console.WriteLine($"Message sent to all connections: {tasks.Length}");
        }
    }
}
