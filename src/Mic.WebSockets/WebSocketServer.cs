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
        private readonly ConcurrentDictionary<int, WebSocketSession> _clients = new ConcurrentDictionary<int, WebSocketSession>();
        private Func<HttpContext, WebSocket, ArraySegment<byte>, Task> _receiverHandlerFunc;
        private Func<HttpContext, WebSocket, Task> _connectedHandlerFunc;
        private Func<HttpContext, WebSocket, Task> _disConnectedHandlerFunc;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="logger"></param>
        public WebSocketServer(ILogger<WebSocketServer> logger)
        {
            _logger = logger;
            _receiverHandlerFunc = (httpContext, webSocket, bytes) => Task.CompletedTask;
            _connectedHandlerFunc = (httpContext, webSocket) => Task.CompletedTask;
            _disConnectedHandlerFunc = (httpContext, webSocket) => Task.CompletedTask;
        }

        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="webSocketId"></param>
        /// <param name="client"></param>
        /// <param name="ext"></param>
        public void AddClient(HttpContext httpContext, int webSocketId, WebSocket client)
        {
            _logger.LogInformation($"{DateTime.Now} WebSocket - {webSocketId} 已连接 -  - {httpContext.Connection.RemoteIpAddress}:{httpContext.Connection.RemotePort}");
            _clients.TryAdd(webSocketId, new WebSocketSession(webSocketId, httpContext, client, null));
            _connectedHandlerFunc.Invoke(httpContext, client);
        }

        /// <summary>
        /// 移除客户端
        /// </summary>
        /// <param name="webSocketId"></param>
        public void RemoveClient(int webSocketId)
        {
            _logger.LogInformation($"{DateTime.Now} WebSocket - {webSocketId} 已断开");
            if (_clients.TryRemove(webSocketId, out WebSocketSession session))
                _disConnectedHandlerFunc.Invoke(session.HttpContext, session.WebSocket);
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
        /// <param name="connectedHandler"></param>
        public void OnConnected(Func<HttpContext, WebSocket, Task> connectedHandler)
        {
            _connectedHandlerFunc = connectedHandler ?? throw new ArgumentNullException(nameof(connectedHandler));
        }

        /// <summary>
        /// 客户端断开连接处理函数
        /// </summary>
        /// <param name="disConnectedHandler"></param>
        public void OnDisConnected(Func<HttpContext, WebSocket, Task> disConnectedHandler)
        {
            _disConnectedHandlerFunc = disConnectedHandler ?? throw new ArgumentNullException(nameof(disConnectedHandler));
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
                await SendAsync(client.Value.WebSocket, message, token);
            }
        }

        /// <summary>
        /// 获取单个客户端
        /// </summary>
        /// <returns></returns>
        public WebSocketSession GetClient(int webSocketId)
        {
            return _clients.TryGetValue(webSocketId, out WebSocketSession session) ? session : null;
        }

        /// <summary>
        /// 获取所有的客户端
        /// </summary>
        /// <returns></returns>
        public List<WebSocketSession> GetAllClients()
        {
            return _clients.Select(d => d.Value).ToList();
        }
    }

    public class WebSocketSession
    {
        public long WebSocketId { get; set; }

        public HttpContext HttpContext { get; set; }

        public WebSocket WebSocket { get; set; }

        /// <summary>
        /// 扩展字段
        /// </summary>
        public object Ext { get; set; }

        public WebSocketSession(long webSocketId, HttpContext httpContext, WebSocket webSocket, object ext)
        {
            WebSocketId = webSocketId;
            HttpContext = httpContext;
            WebSocket = webSocket;
            Ext = ext;
        }
    }
}