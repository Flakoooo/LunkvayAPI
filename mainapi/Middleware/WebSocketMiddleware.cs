using LunkvayAPI.Common.Interfaces;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace LunkvayAPI.Middleware
{
    public class WebSocketMiddleware(RequestDelegate next, IWebSocketConnectionManager connectionManager)
    {
        private readonly RequestDelegate _next = next;
        private readonly IWebSocketConnectionManager _connectionManager = connectionManager;

        public class WebSocketCommand
        {
            public string Type { get; set; } = string.Empty;
            public Guid? RoomId { get; set; }
            public object? Data { get; set; }
        }

        public class WebSocketResponse
        {
            public string Type { get; set; } = string.Empty;
            public object? Data { get; set; }
            public bool Success { get; set; } = true;
            public string? Error { get; set; }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
            {
                await HandleWebSocketConnection(context);
            }
            else
            {
                await _next(context);
            }
        }

        private async Task HandleWebSocketConnection(HttpContext context)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var connectionId = Guid.NewGuid().ToString();

            Console.WriteLine($"WebSocket connection established: {connectionId}");

            try
            {
                await HandleWebSocketMessages(webSocket, connectionId);
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
            finally
            {
                _connectionManager.RemoveConnection(connectionId);
                Console.WriteLine($"WebSocket connection closed: {connectionId}");
            }
        }

        private async Task HandleWebSocketMessages(WebSocket webSocket, string connectionId)
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessWebSocketMessage(message, webSocket, connectionId);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                    break;
                }
            }
        }

        private async Task ProcessWebSocketMessage(string message, WebSocket webSocket, string connectionId)
        {
            try
            {
                var command = JsonSerializer.Deserialize<WebSocketCommand>(message);
                if (command == null) return;

                switch (command.Type)
                {
                    case "JoinRoom":
                        if (command.RoomId.HasValue)
                        {
                            _connectionManager.AddConnection(command.RoomId.Value, webSocket, connectionId);
                            var response = new WebSocketResponse
                            {
                                Type = "JoinedRoom",
                                Data = new { RoomId = command.RoomId.Value }
                            };
                            await SendWebSocketMessage(webSocket, response);
                        }
                        break;

                    case "LeaveRoom":
                        if (command.RoomId.HasValue)
                        {
                            _connectionManager.RemoveConnection(connectionId);
                            var response = new WebSocketResponse
                            {
                                Type = "LeftRoom",
                                Data = new { RoomId = command.RoomId.Value }
                            };
                            await SendWebSocketMessage(webSocket, response);
                        }
                        break;

                    default:
                        var errorResponse = new WebSocketResponse
                        {
                            Type = "Error",
                            Success = false,
                            Error = $"Unknown command: {command.Type}"
                        };
                        await SendWebSocketMessage(webSocket, errorResponse);
                        break;
                }
            }
            catch (JsonException ex)
            {
                var errorResponse = new WebSocketResponse
                {
                    Type = "Error",
                    Success = false,
                    Error = $"Invalid JSON: {ex.Message}"
                };
                await SendWebSocketMessage(webSocket, errorResponse);
            }
        }

        private static async Task SendWebSocketMessage(WebSocket webSocket, object message)
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(bytes);

            await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
