using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Mic.WebSockets
{
    public class WebSocketController : ControllerBase
    {
        private readonly IWebSocketServer _server;
        private readonly ILogger<WebSocketController> _logger;

        private int _webSocketId = 0;

        public WebSocketController(IWebSocketServer server, ILogger<WebSocketController> logger)
        {
            _server = server;
            _logger = logger;
        }

#if NET7_0_OR_GREATER

        [Route("/ws")]
#else
        [HttpGet("/ws")]
#endif
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                _webSocketId = webSocket.GetHashCode();

                _server.AddClient(HttpContext, _webSocketId, webSocket);

                await Echo(HttpContext, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private async Task Echo(HttpContext httpContext, WebSocket webSocket)
        {
            try
            {
                var buffer = new byte[WebSocketServerExtension.BufferLength];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var allBuffer = new List<byte>();
                while (!result.CloseStatus.HasValue)
                {
                    var bytes = new ArraySegment<byte>(buffer, 0, result.Count);
                    
                    //确保消息接收完整
                    if (result.EndOfMessage == false)
                    {
                        allBuffer.AddRange(bytes);
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        continue;
                    }

                    if (allBuffer.Any())
                    {
                        allBuffer.AddRange(bytes);
                        await _server.Receiver(httpContext, webSocket, new ArraySegment<byte>(allBuffer.ToArray()));
                        allBuffer.Clear();
                    }
                    else
                    {
                        await _server.Receiver(httpContext, webSocket, bytes);
                    }

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.LogError($"{DateTime.Now} WebSocket 报错：{e.Message}", e);
            }
            finally
            {
                _server.RemoveClient(_webSocketId);
            }
        }
    }
}
