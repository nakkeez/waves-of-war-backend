using System.Net.WebSockets;

namespace WavesOfWarServer.Services
{
    /// <summary>
    /// Service to handle WebSocket connections and communication.
    /// </summary>
    public class WebSocketService
    {
        private static readonly List<WebSocket> _sockets = [];

        /// <summary>
        /// Handles an incoming WebSocket request.
        /// If the request is a WebSocket request, it accepts the connection and starts listening for messages.
        /// If not, it returns a 400 Bad Request status code.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="logger">The logger to log information and errors.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task HandleWebSocketAsync(HttpContext context, ILogger logger)
        {

            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                lock (_sockets)
                {
                    _sockets.Add(webSocket);
                }
                await Echo(webSocket, logger);
                lock (_sockets)
                {
                    _sockets.Remove(webSocket);
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        /// <summary>
        /// Echoes messages received from the WebSocket back to all connected clients.
        /// </summary>
        /// <param name="webSocket">The WebSocket to receive messages from.</param>
        /// <param name="logger">The logger to log information and errors.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private static async Task Echo(WebSocket webSocket, ILogger logger)
        {
            var buffer = new byte[1024 * 4];
            try
            {
                var receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!receiveResult.CloseStatus.HasValue)
                {
                    var message = new ArraySegment<byte>(buffer, 0, receiveResult.Count);

                    // Copy the clients to list so lock can be released before broadcasting the messages
                    List<WebSocket> socketsCopy;
                    lock (_sockets)
                    {
                        socketsCopy = [.. _sockets];
                    }

                    foreach (WebSocket socket in socketsCopy)
                    {
                        if (socket.State == WebSocketState.Open)
                        {
                            await socket.SendAsync(
                                message,
                                receiveResult.MessageType,
                                receiveResult.EndOfMessage,
                                CancellationToken.None);
                        }
                    }

                    receiveResult = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await webSocket.CloseAsync(
                    receiveResult.CloseStatus.Value,
                    receiveResult.CloseStatusDescription,
                    CancellationToken.None);

            }
            catch (WebSocketException ex)
            {
                logger.LogError("WebSocketException: {Message}", ex.Message);
            }
        }
    }
}
