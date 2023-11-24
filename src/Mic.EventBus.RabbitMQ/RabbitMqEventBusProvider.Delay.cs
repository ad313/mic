using Mic.EventBus.RabbitMQ.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mic.EventBus.RabbitMQ
{
    /// <summary>
    /// 基于 RabbitMQ 的发布订阅实现
    /// </summary>
    public partial class RabbitMqEventBusProvider
    {
        /// <summary>
        /// 发布事件 延迟队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="seconds">延迟秒数</param>
        /// <param name="message">数据</param>
        /// <returns></returns>
        public async Task DelayPublishAsync<T>(string key, long seconds, EventMessageModel<T> message)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (seconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));

            //附加 http header
            Common.AppendHttpHeader(message, ServiceProvider);

            var channel = _rabbitMqClientProvider.GetChannel();
            channel.ConfirmSelect();

            var dic = new Dictionary<string, object>
            {
                {"x-expires", (seconds + 60) * 1000},
                {"x-message-ttl", seconds * 1000}, //队列上消息过期时间，应小于队列过期时间  
                {"x-dead-letter-exchange", _config.DeadLetterExchange}, //过期消息转向路由  
                {"x-dead-letter-routing-key", _config.GetDeadLetterRouteKey(key)} //过期消息转向路由相匹配routingkey  
            };

            var queueKey = _config.GetDeadLetterHostQueueKey(key, seconds);

            channel.QueueDeclare(queue: queueKey,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: dic);

            message.Key = key;
            var body = _serializerProvider.SerializeBytes(message);

            //持久化
            var properties = channel.BasicProperties();

            //向该消息队列发送消息message
            channel.BasicPublish(exchange: "",
                routingKey: queueKey,
                basicProperties: properties,
                body: body);

            channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

            _logger.LogDebug($"RabbitMQ topic message [{queueKey}] has been published. message:{_serializerProvider.Serialize(message)}");

            _rabbitMqClientProvider.ReturnChannel(channel);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 发布事件 延迟队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="absoluteTime">指定执行时间</param>
        /// <param name="message">数据</param>
        /// <returns></returns>
        public async Task DelayPublishAsync<T>(string key, DateTime absoluteTime, EventMessageModel<T> message)
        {
            var seconds = (long)(absoluteTime - DateTime.Now).TotalSeconds;
            if (seconds <= 0)
                throw new ArgumentException("absoluteTime must be greater than current time");

            await DelayPublishAsync(key, seconds + 1, message);
        }

        /// <summary>
        /// 订阅事件 延迟队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="handler">订阅处理</param>
        public void DelaySubscribe<T>(string key, Action<EventMessageModel<T>> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            (IModel channel, string queue, string routeKey) resource = GetDelayResource(key);

            var consumer = new EventingBasicConsumer(resource.channel);
            consumer.Received += (ch, ea) =>
            {
                try
                {
                    if (!IsEnable(key))
                    {
                        _logger.LogWarning($"{DateTime.Now} 频道【{resource.routeKey}】 已关闭消费");
                        return;
                    }

                    using var scope = ServiceProvider.CreateScope();
                    var data = GetResultFromBody<T>(key, ea, scope.ServiceProvider);
                    handler.Invoke(data);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"{DateTime.Now} RabbitMQ [{resource.routeKey}] 消费异常 {e.Message} ");
                }
                finally
                {
                    resource.channel.BasicAck(ea.DeliveryTag, false);
                }
            };

            resource.channel.BasicConsume(queue: resource.queue, autoAck: false, consumer: consumer);
        }

        /// <summary>
        /// 订阅事件 延迟队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="handler">订阅处理</param>
        public void DelaySubscribe<T>(string key, Func<EventMessageModel<T>, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            (IModel channel, string queue, string routeKey) resource = GetDelayResource(key);

            var consumer = new EventingBasicConsumer(resource.channel);
            consumer.Received += async (ch, ea) =>
            {
                try
                {
                    if (!IsEnable(key))
                    {
                        _logger.LogWarning($"{DateTime.Now} 频道【{resource.routeKey}】 已关闭消费");
                        return;
                    }

                    await using var scope = ServiceProvider.CreateAsyncScope();
                    var data = GetResultFromBody<T>(key, ea, scope.ServiceProvider);
                    await handler.Invoke(data);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"{DateTime.Now} RabbitMQ [{resource.routeKey}] 消费异常 {e.Message} ");
                }
                finally
                {
                    resource.channel.BasicAck(ea.DeliveryTag, false);
                }
            };

            resource.channel.BasicConsume(queue: resource.queue, autoAck: false, consumer: consumer);
        }

        private (IModel, string, string) GetDelayResource(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            _logger.LogWarning($"{DateTime.Now} RabbitMQ 开始订阅 死信队列 " +
                               $"[{_config.DeadLetterExchange}] " +
                               $"[{_config.GetDeadLetterWorkQueueKey(key)}] " +
                               $"[PrefetchSize：{_config.PrefetchSize}]" +
                               $"[PrefetchCount：{_config.PrefetchCount}]" +
                               $"......");

            var channel = _rabbitMqClientProvider.Channel;

            channel.ExchangeDeclare(exchange: _config.DeadLetterExchange, type: "direct");
            var queue = _config.GetDeadLetterWorkQueueKey(key);
            var routeKey = _config.GetDeadLetterRouteKey(key);
            channel.QueueDeclare(queue, true, false, false, null);
            channel.QueueBind(queue: queue, exchange: _config.DeadLetterExchange, routingKey: routeKey);

            //限流
            channel.BasicQos(_config.PrefetchSize, _config.PrefetchCount, false);

            return (channel, queue, routeKey);
        }
    }
}