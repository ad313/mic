using Mic.EventBus.RabbitMQ.Core.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mic.EventBus.RabbitMQ
{
    /// <summary>
    /// 基于 RabbitMQ 的发布订阅实现
    /// </summary>
    public partial class RabbitMqEventBusProvider 
    {
        public async Task<RpcResult<T>> RpcClientAsync<T>(string key, object[] message = null, int timeout = 30)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var channel = _rabbitMqClientProvider.GetChannel();
            channel.ConfirmSelect();

            //var replyQueueName = channel.QueueDeclare(_config.GetRpcClientQueueKey(key)).QueueName;
            var replyQueueName = channel.QueueDeclare().QueueName;
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                _rabbitMqClientProvider.ReturnChannel(channel);

                if (!_rpcCallbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<RpcResult> tcs))
                    return;

                tcs.TrySetResult(_serializerProvider.Deserialize<RpcResult>(ea.Body.ToArray()));
            };

            //附加 http header
            message ??= new Object[] { };
            var data = (message ?? new object[] { }).ToList();
            data.Add(Common.GetHttpHeader(ServiceProvider));

            //message[message.Length] = Common.GetHttpHeader(ServiceProvider);

            var result = await RpcClientCallAsync(key, channel, consumer, replyQueueName, data.ToArray(), timeout);
            _logger.LogDebug($"RabbitMQ rpc client [{key}] receive {_serializerProvider.Serialize(result)}");

            await Task.Delay(1);

            return new RpcResult<T>()
            {
                Data = typeof(T) == typeof(string)
                    ? (T)((object)result.Data)
                    : (string.IsNullOrWhiteSpace(result.Data)
                        ? default(T)
                        : _serializerProvider.Deserialize<T>(result.Data)),
                Success = result.Success,
                ErrorMessage = result.ErrorMessage
            };
        }

        /// <summary>
        /// 订阅事件 RpcServer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="handler">订阅处理</param>
        public void RpcServer<T>(string key, Func<T, Task<RpcResult>> handler)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _logger.LogWarning($"{DateTime.Now} RabbitMQ RpcServer 启动 " +
                               $"[{key}] " +
                               $"......");

            var channel = _rabbitMqClientProvider.Channel;

            channel.QueueDeclare(queue: _config.GetRpcServerQueueKey(key), durable: false, exclusive: false, autoDelete: true, arguments: null);

            //channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: _config.GetRpcServerQueueKey(key), autoAck: false, consumer: consumer);
            consumer.Received += async (model, ea) =>
            {
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                RpcResult response = null;

                try
                {
                    _logger.LogDebug($"{DateTime.Now}：RpcServer [{key}] 收到消息： {Encoding.Default.GetString(ea.Body.ToArray())}");

                    response = await handler.Invoke(_serializerProvider.Deserialize<T>(ea.Body.ToArray()));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"{DateTime.Now} RabbitMQ RpcServer [{key}] 执行异常 {e.Message} ");

                    response = new RpcResult($"RpcServer [{key}] 执行异常", e);
                }
                finally
                {
                    channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: _serializerProvider.SerializeBytes(response));
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            };
        }

        private Task<RpcResult> RpcClientCallAsync(string key, IModel channel, EventingBasicConsumer consumer, string queueName, object[] message, int timeout)
        {
            timeout = timeout > 0 ? timeout : 120;
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));

            var correlationId = Guid.NewGuid().ToString();

            var tcs = new TaskCompletionSource<RpcResult>();
            _rpcCallbackMapper.TryAdd(correlationId, tcs);

            var props = channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = queueName;

            channel.BasicPublish(
                exchange: "",
                routingKey: _config.GetRpcServerQueueKey(key),
                basicProperties: props,
                body: _serializerProvider.SerializeBytes(message));

            channel.BasicConsume(
                consumer: consumer,
                queue: queueName,
                autoAck: true);

            channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

            _logger.LogDebug($"RabbitMQ rpc client [{key}] has been send. message:{_serializerProvider.Serialize(message)}");

            tokenSource.Token.Register(() =>
            {
                if (_rpcCallbackMapper.TryRemove(correlationId, out var tmp))
                    tmp.SetResult(new RpcResult(null, new TimeoutException($"超时时间 {timeout} s")));
            });

            return tcs.Task;
        }
    }
}