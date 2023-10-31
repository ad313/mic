using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mic.WebSockets
{
    public class WebSocketServer : IWebSocketServer
    {
        private readonly ILogger<WebSocketServer> _logger;
        private readonly ConcurrentDictionary<int, WebSocket> _clients = new ConcurrentDictionary<int, WebSocket>();
        private Func<HttpContext, WebSocket, ArraySegment<byte>, Task> _receiverHandlerFunc;
        private Func<HttpContext, WebSocket, Task> _connectionHandlerFunc;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="logger"></param>
        public WebSocketServer(ILogger<WebSocketServer> logger)
        {
            _logger = logger;
            _receiverHandlerFunc = (httpContext, webSocket, bytes) => Task.CompletedTask;
            _connectionHandlerFunc = (httpContext, webSocket) => Task.CompletedTask;
        }

        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="webSocketId"></param>
        /// <param name="client"></param>
        public void AddClient(HttpContext httpContext, int webSocketId, WebSocket client)
        {
            _logger.LogInformation($"{DateTime.Now} WebSocket - {webSocketId} 已连接 -  - {httpContext.Connection.RemoteIpAddress}:{httpContext.Connection.RemotePort}");
            _clients.TryAdd(webSocketId, client);
            _connectionHandlerFunc.Invoke(httpContext, client);
        }

        /// <summary>
        /// 移除客户端
        /// </summary>
        /// <param name="webSocketId"></param>
        public void RemoveClient(int webSocketId)
        {
            _logger.LogInformation($"{DateTime.Now} WebSocket - {webSocketId} 已断开");
            _clients.TryRemove(webSocketId, out _);
        }

        /// <summary>
        /// 收到消息
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="webSocket"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task Receiver(HttpContext httpContext, WebSocket webSocket, ArraySegment<byte> bytes)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now} - {webSocket.GetHashCode()} - {httpContext.Connection.RemoteIpAddress}:{httpContext.Connection.RemotePort} 收到消息：{bytes.Count} 字节");
                _logger.LogDebug($"{DateTime.Now} - {webSocket.GetHashCode()} 收到消息：{Encoding.UTF8.GetString(bytes)}");

                await _receiverHandlerFunc(httpContext, webSocket, bytes);
            }
            catch (Exception e)
            {
                _logger.LogError($"{DateTime.Now} - {webSocket.GetHashCode()} - {httpContext.Connection.RemoteIpAddress}:{httpContext.Connection.RemotePort} 接收消息处理失败：{e.Message}", e);
            }
        }

        /// <summary>
        /// 客户端连接处理函数
        /// </summary>
        /// <param name="connectionHandler"></param>
        public void OnConnectioned(Func<HttpContext, WebSocket, Task> connectionHandler)
        {
            _connectionHandlerFunc = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
        }

        /// <summary>
        /// 收到消息处理函数
        /// </summary>
        /// <param name="receiverHandler"></param>
        public void OnReceived(Func<HttpContext, WebSocket, ArraySegment<byte>, Task> receiverHandler)
        {
            _receiverHandlerFunc = receiverHandler ?? throw new ArgumentNullException(nameof(receiverHandler));
        }

        /// <summary>
        /// 给单个客户端发送消息
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task SendAsync(WebSocket webSocket, string message, CancellationToken token = default)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            await webSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, token);
        }

        /// <summary>
        /// 给单个客户端发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="webSocket"></param>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task SendAsync<T>(WebSocket webSocket, T message, CancellationToken token = default) where T : class
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            await webSocket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(message), WebSocketMessageType.Text, true, token);
        }

        /// <summary>
        /// 给所以客户端发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task SendToAllClientsAsync<T>(T message, CancellationToken token = default) where T : class
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            foreach (var client in _clients)
            {
                await SendAsync(client.Value, message, token);
            }
        }

        /// <summary>
        /// 获取所有的客户端
        /// </summary>
        /// <returns></returns>
        public List<WebSocket> GetAllClients()
        {
            return _clients.Select(d => d.Value).ToList();
        }
    }
}