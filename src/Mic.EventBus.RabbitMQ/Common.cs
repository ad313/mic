using Mic.EventBus.RabbitMQ.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mic.EventBus.RabbitMQ
{
    internal class Common
    {
        /// <summary>
        /// 附加 http header。用于传递token
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static void AppendHttpHeader<T>(EventMessageModel<T> message, IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var httpContext = scope.ServiceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
            if (httpContext == null)
                return;

            message.SetHttpHeader(httpContext.Request?.Headers);
        }

        public static Dictionary<string, string> GetHttpHeader(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var httpContext = scope.ServiceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
            return httpContext?.Request?.Headers.ToDictionary(d => d.Key, d => d.Value.ToString()) ?? new Dictionary<string, string>();
        }


        /// <summary>
        /// 添加 Http Header 到 HttpContext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scopeServiceProvider"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static EventMessageModel<T> MergeHttpHeaderToHttpContext<T>(IServiceProvider scopeServiceProvider, EventMessageModel<T> data)
        {
            //处理 Http Header
            var accessor = scopeServiceProvider.GetService<IHttpContextAccessor>();
            if (data.HttpHeader?.Count > 0)
            {
                accessor.HttpContext ??= new DefaultHttpContext();

                foreach (var item in data.HttpHeader)
                {
                    if (accessor.HttpContext.Request.Headers.Keys.Contains(item.Key))
                        continue;

                    accessor.HttpContext.Request.Headers.Add(item.Key, item.Value);
                }
            }

            return data;
        }

        /// <summary>
        /// 添加 Http Header 到 HttpContext
        /// </summary>
        /// <param name="scopeServiceProvider"></param>
        /// <param name="jsonElement"></param>
        /// <returns></returns>
        public static void MergeHttpHeaderToHttpContext(IServiceProvider scopeServiceProvider, JsonElement jsonElement)
        {
            try
            {
                var json = JsonSerializer.Serialize(jsonElement);
                if (string.IsNullOrWhiteSpace(json))
                    return;

                var dic = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                //处理 Http Header
                var accessor = scopeServiceProvider.GetService<IHttpContextAccessor>();
                if (dic?.Count > 0)
                {
                    accessor.HttpContext ??= new DefaultHttpContext();

                    foreach (var item in dic)
                    {
                        if (accessor.HttpContext.Request.Headers.Keys.Contains(item.Key))
                            continue;

                        accessor.HttpContext.Request.Headers.Add(item.Key, item.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"RpcServer 添加 Http Header 到 HttpContext 错误：{e.Message}", e);
            }
        }



        public static string FormatBroadcastKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return $"aop_cache_broadcast_{key}";
        }

        public static async Task HandleError<T>(ILogger<RabbitMqEventBusProvider> logger, string channel, List<T> data, Func<Exception, List<T>, Task> error, Exception e)
        {
            try
            {
                await error.Invoke(e, data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{channel} error func 执行异常：{ex.Message}");
            }
        }

        public static async Task HandleCompleted(ILogger<RabbitMqEventBusProvider> logger, string channel, Func<Task> completed)
        {
            try
            {
                await completed.Invoke();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{channel} Completed func 执行异常：{ex.Message}");
            }
        }


    }
}
