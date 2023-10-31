using Mic.WebSockets;
using Microsoft.AspNetCore.Builder;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// WebSocket 扩展服务
    /// </summary>
    public static class WebSocketServerExtension
    {
        public static int BufferLength { get; private set; } = 1024 * 4;

        public static IServiceCollection AddWebSockets(this IServiceCollection service)
        {
            service.AddSingleton<IWebSocketServer, WebSocketServer>();

            return service;
        }

        public static IApplicationBuilder UseWebSockets(this IApplicationBuilder app, int bufferLength = 1024 * 4, int keepAliveIntervalSeconds = 120)
        {
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(keepAliveIntervalSeconds)
            };

            BufferLength = bufferLength;

            if (BufferLength <= 0)
                BufferLength = 1024 * 4;

            app.UseWebSockets(webSocketOptions);

            return app;
        }

        public static IApplicationBuilder UseWebSockets(this IApplicationBuilder app, int bufferLength, WebSocketOptions webSocketOptions)
        {
            BufferLength = bufferLength;

            if (BufferLength <= 0)
                BufferLength = 1024 * 4;

            if (webSocketOptions != null)
                app.UseWebSockets(webSocketOptions);

            return app;
        }
    }
}