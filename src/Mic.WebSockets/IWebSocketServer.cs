using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Mic.WebSockets
{
    public interface IWebSocketServer
    {
        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="webSocketId"></param>
        /// <param name="client"></param>
        void AddClient(HttpContext httpContext, int webSocketId, WebSocket client);

        /// <summary>
        /// 移除客户端
        /// </summary>
        /// <param name="webSocketId"></param>
        void RemoveClient(int webSocketId);

        /// <summary>
        /// 收到消息
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="webSocket"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        Task Receiver(HttpContext httpContext, WebSocket webSocket, ArraySegment<byte> bytes);

        /// <summary>
        /// 客户端连接处理函数
        /// </summary>
        /// <param name="connectionHandler"></param>
        void OnConnectioned(Func<HttpContext, WebSocket, Task> connectionHandler);

        /// <summary>
        /// 收到消息处理函数
        /// </summary>
        /// <param name="receiverHandler"></param>
        void OnReceived(Func<HttpContext, WebSocket, ArraySegment<byte>, Task> receiverHandler);

        /// <summary>
        /// 给单个客户端发送消息
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task SendAsync(WebSocket webSocket, string message, CancellationToken token = default);

        /// <summary>
        /// 给单个客户端发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="webSocket"></param>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task SendAsync<T>(WebSocket webSocket, T message, CancellationToken token = default) where T : class;

        /// <summary>
        /// 给所以客户端发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task SendToAllClientsAsync<T>(T message, CancellationToken token = default) where T : class;

        /// <summary>
        /// 获取所有的客户端
        /// </summary>
        /// <returns></returns>
        List<WebSocket> GetAllClients();
    }
}