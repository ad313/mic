using Mic.EventBus.RabbitMQ.Core.Abstractions;
using Mic.Helpers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mic.EventBus.RabbitMQ
{
    /// <summary>
    /// 基于 RabbitMQ 的发布订阅实现
    /// </summary>
    public partial class RabbitMqEventBusProvider 
    {
        /// <summary>
        /// 订阅事件 从队列读取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="handler">订阅处理</param>
        public void SubscribeQueue<T>(string key, Action<Func<int, List<T>>> handler)
        {
            SubscribeBroadcastInternal<T>(key, msg =>
            {
                List<T> GetListFunc(int length) => GetQueueItems<T>(key, length);
                handler.Invoke(GetListFunc);
            }, true);
        }

        /// <summary>
        /// 订阅事件 从队列读取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="handler">订阅处理</param>
        public void SubscribeQueue<T>(string key, Func<Func<int, Task<List<T>>>, Task> handler)
        {
            SubscribeBroadcastInternal<T>(key, async msg =>
            {
                Task<List<T>> GetListFunc(int length) => GetQueueItemsAsync<T>(key, length);
                await handler.Invoke(GetListFunc);
            }, true);
        }

        /// <summary>
        /// 订阅事件 从队列读取数据 分批次消费
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="length">每次处理条数</param>
        /// <param name="delay">每次处理间隔 毫秒</param>
        /// <param name="exceptionHandler">异常处理方式</param>
        /// <param name="handler">订阅处理</param>
        /// <param name="error">发生异常时回调</param>
        /// <param name="completed">本次消费完成回调 最后执行</param>
        public void SubscribeQueue<T>(string key, int length, int delay, ExceptionHandlerEnum exceptionHandler, Func<List<T>, Task> handler,
            Func<Exception, List<T>, Task> error = null, Func<Task> completed = null)
        {
            if (length <= 0)
                throw new Exception("length must be greater than zero");

            SubscribeBroadcastInternal<T>(key, async msg =>
            {
                while (true)
                {
                    List<T> data = null;
                    Exception ex = null;
                    var isCompleted = false;

                    try
                    {
                        data = await GetQueueItemsAsync<T>(key, length);
                        if (!data.Any())
                        {
                            isCompleted = true;
                            return;
                        }

                        await handler.Invoke(data);
                    }
                    catch (Exception e)
                    {
                        ex = e;

                        if (await HandleException(key, exceptionHandler, data, e))
                            return;
                    }
                    finally
                    {
                        if (ex != null && error != null)
                            await Common.HandleError(_logger, key, data, error, ex);

                        if (completed != null && isCompleted)
                            await Common.HandleCompleted(_logger, key, completed);
                    }

                    if (isCompleted)
                        break;

                    if (delay > 0)
                        await Task.Delay(delay);
                }
            }, true);

            _lockObjectDictionary.TryAdd(key, (AsyncLock)new AsyncLock());
        }

        private async Task PushToQueueAsync<T>(string key, List<T> data, int length = 1000)
        {
            if (data == null || !data.Any())
                return;

            var queueName = GetChannelQueueKey(key);
            await PushToQueueAsyncMethod(queueName, data, length);
        }

        private async Task PushToErrorQueueAsync<T>(string key, List<T> data, int length = 10000)
        {
            if (data == null || !data.Any())
                return;

            var queueName = GetChannelErrorQueueKey(key);
            await PushToQueueAsyncMethod(queueName, data, length);
        }

        private async Task PushToQueueAsyncMethod<T>(string queueName, List<T> data, int length = 10000)
        {
            if (data == null || !data.Any())
                return;

            var channel = _rabbitMqClientProvider.GetChannel();
            channel.QueueDeclare(queueName, true, false, false, null);

            //持久化
            var properties = channel.BasicProperties();

            if (data.Count > length)
            {
                foreach (var list in ListHelper.SplitList(data, length))
                {
                    foreach (var item in list)
                    {
                        channel.BasicPublish("", queueName, properties, _serializerProvider.SerializeBytes(item));
                    }
                    await Task.Delay(10);
                }
            }
            else
            {
                foreach (var item in data)
                {
                    channel.BasicPublish("", queueName, properties, _serializerProvider.SerializeBytes(item));
                }
            }

            _rabbitMqClientProvider.ReturnChannel(channel);
        }

        private async Task<bool> HandleException<T>(string channel, ExceptionHandlerEnum exceptionHandler, List<T> data, Exception e)
        {
            try
            {
                var text = $"{DateTime.Now} {channel} 队列消费端异常：{e.Message}";
                _logger.LogError(e, text);

                switch (exceptionHandler)
                {
                    case ExceptionHandlerEnum.Continue:
                        return false;
                    case ExceptionHandlerEnum.Stop:
                        return true;
                    case ExceptionHandlerEnum.PushToSelfQueueAndStop:
                        await PushToQueueAsync(channel, data);
                        return true;
                    case ExceptionHandlerEnum.PushToSelfQueueAndContinue:
                        await PushToQueueAsync(channel, data);
                        return false;
                    case ExceptionHandlerEnum.PushToErrorQueueAndStop:
                        await PushToErrorQueueAsync(channel, data);
                        return true;
                    case ExceptionHandlerEnum.PushToErrorQueueAndContinue:
                        await PushToErrorQueueAsync(channel, data);
                        return false;
                    default:
                        _logger.LogError($"{DateTime.Now} 不支持的 ExceptionHandlerEnum 类型：{exceptionHandler}");
                        return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{channel} 异常处理异常：{ex.Message}");
                return true;
            }
        }
    }
}