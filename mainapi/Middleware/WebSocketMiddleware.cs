using LunkvayAPI.Common.Interfaces;
using System.Net.WebSockets;
using System.Text;

namespace LunkvayAPI.Middleware
{
    public class WebSocketMiddleware(RequestDelegate next, IWebSocketConnectionManager connectionManager)
    {
        private readonly RequestDelegate _next = next;
        private readonly IWebSocketConnectionManager _connectionManager = connectionManager;

        private static Guid? GetRoomIdFromContext(HttpContext context)
        {
            if (context.Request.Query.TryGetValue("roomId", out var roomIdStr) &&
                Guid.TryParse(roomIdStr, out var roomId)) return roomId;

            if (context.Request.Headers.TryGetValue("X-Room-Id", out var headerRoomId) &&
                Guid.TryParse(headerRoomId, out var roomIdFromHeader)) return roomIdFromHeader;

            return null;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
                await HandleWebSocketConnection(context);
            else
                await _next(context);
        }

        private async Task HandleWebSocketConnection(HttpContext context)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var connectionId = Guid.NewGuid().ToString();

            var roomId = GetRoomIdFromContext(context);

            if (roomId.HasValue)
            {
                _connectionManager.AddConnection(roomId.Value, webSocket, connectionId);
                Console.WriteLine($"Client {connectionId} joined room {roomId}");
            }
            else
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation,
                    "RoomId is required", CancellationToken.None);
                return;
            }

            await HandleWebSocketMessages(webSocket, connectionId);
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
                    Console.WriteLine($"Received message from {connectionId}: {message}");
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _connectionManager.RemoveConnection(connectionId);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                    break;
                }
            }
        }
    }
}
