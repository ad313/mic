using Mic.EventBus.RabbitMQ.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace Mic.EventBus.RabbitMQ
{
    /// <summary>
    /// 基于 RabbitMQ 的发布订阅实现
    /// </summary>
    public partial class RabbitMqEventBusProvider 
    {
        private async Task PublishBroadcastAsync<T>(string key, EventMessageModel<T> message)
        {
            key = Common.FormatBroadcastKey(key);

            var channel = _rabbitMqClientProvider.GetChannel();
            channel.ConfirmSelect();
            channel.ExchangeDeclare(key, "fanout");

            message.Key = key;
            var body = _serializerProvider.SerializeBytes(message);

            channel.BasicPublish(exchange: key,
                routingKey: "",
                basicProperties: null,
                body: body);

            channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

            _logger.LogDebug($"RabbitMQ broadcast message [{key}] has been published. message:{_serializerProvider.Serialize(message)}");

            _rabbitMqClientProvider.ReturnChannel(channel);

            await Task.CompletedTask;
        }

        private void SubscribeBroadcastInternal<T>(string key, Action<EventMessageModel<T>> handler, bool checkEnable = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            key = Common.FormatBroadcastKey(key);

            _logger.LogWarning($"{DateTime.Now} RabbitMQ 广播模式 开始订阅 " +
                               $"[{key}] " +
                               $"......");

            var channel = _rabbitMqClientProvider.Channel;
            channel.ExchangeDeclare(key, "fanout");

            //队列
            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, key, "");

            //限流
            channel.BasicQos(_config.PrefetchSize, _config.PrefetchCount, false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                try
                {
                    if (checkEnable && !IsEnable(key))
                    {
                        _logger.LogWarning($"{DateTime.Now} 频道【{key}】 已关闭消费");
                        return;
                    }

                    using var scope = ServiceProvider.CreateScope();
                    var data = GetResultFromBody<T>(key, ea, scope.ServiceProvider);
                    handler.Invoke(data);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"{DateTime.Now} RabbitMQ [{key}] 消费异常 {e.Message} ");
                }
                finally
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            };

            channel.BasicConsume(queueName, false, consumer);
        }

        private void SubscribeBroadcastInternal<T>(string key, Func<EventMessageModel<T>, Task> handler, bool checkEnable = true)
        {
            Task.Factory.StartNew(() =>
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));

                if (handler == null)
                    throw new ArgumentNullException(nameof(handler));

                key = Common.FormatBroadcastKey(key);

                _logger.LogWarning($"{DateTime.Now} RabbitMQ 广播模式 开始订阅 " +
                                   $"[{key}] " +
                                   $"......");

                var channel = _rabbitMqClientProvider.Channel;
                channel.ExchangeDeclare(key, "fanout");

                //队列
                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queueName, key, "");

                //限流
                channel.BasicQos(_config.PrefetchSize, _config.PrefetchCount, false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (ch, ea) =>
                {
                    try
                    {
                        if (checkEnable && !IsEnable(key))
                        {
                            _logger.LogWarning($"{DateTime.Now} 频道【{key}】 已关闭消费");
                            return;
                        }

                        await using var scope = ServiceProvider.CreateAsyncScope();
                        var data = GetResultFromBody<T>(key, ea, scope.ServiceProvider);
                        await handler.Invoke(data);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"{DateTime.Now} RabbitMQ [{key}] 消费异常 {e.Message} ");
                    }
                    finally
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                };

                channel.BasicConsume(queueName, false, consumer);
            }, TaskCreationOptions.LongRunning);
        }
    }
}