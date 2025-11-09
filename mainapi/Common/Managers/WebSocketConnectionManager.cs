using LunkvayAPI.Common.Interfaces;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LunkvayAPI.Common.Managers
{
    public class WebSocketConnectionManager : IWebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, WebSocket>> _roomConnections = new();
        private readonly ConcurrentDictionary<string, Guid> _connectionRooms = new();
        private readonly Lazy<JsonSerializerOptions> _jsonOptions;

        public WebSocketConnectionManager()
        {
            _jsonOptions = new Lazy<JsonSerializerOptions>(CreateJsonOptions);
        }

        private class WebSocketDateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TryGetDateTime(out DateTime date))
                    return date;

                if (DateTime.TryParse(reader.GetString(), out date))
                    return date;

                return default;
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"));
        }

        private static JsonSerializerOptions CreateJsonOptions() => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new WebSocketDateTimeConverter() }
        };

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
                var json = JsonSerializer.Serialize(message, _jsonOptions.Value);
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
            var json = JsonSerializer.Serialize(message, _jsonOptions.Value);
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
