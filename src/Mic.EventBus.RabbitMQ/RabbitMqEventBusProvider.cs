using Mic.EventBus.RabbitMQ.Core.Abstractions;
using Mic.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mic.EventBus.RabbitMQ
{
    /// <summary>
    /// 基于 RabbitMQ 的发布订阅实现
    /// </summary>
    public partial class RabbitMqEventBusProvider : IEventBusProvider
    {
        /// <summary>
        /// ServiceProvider
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        private readonly ISerializerProvider _serializerProvider;
        private readonly RabbitMqClientProvider _rabbitMqClientProvider;
        private readonly RabbitMqClientProvider _rabbitMqClientProviderRead;
        private readonly RabbitMqConfig _config;
        private readonly ILogger<RabbitMqEventBusProvider> _logger;

        /// <summary>
        /// 总开关默认开启
        /// </summary>
        public bool Enable { get; private set; } = true;

        /// <summary>
        /// 频道开关
        /// </summary>
        private readonly ConcurrentDictionary<string, bool> _channelEnableDictionary = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// key queue 名称字典
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _queueNameDictionary = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 异步锁
        /// </summary>
        private readonly ConcurrentDictionary<string, AsyncLock> _lockObjectDictionary = new ConcurrentDictionary<string, AsyncLock>();

        /// <summary>
        /// Rpc client 标识
        /// </summary>
        private readonly ConcurrentDictionary<string, TaskCompletionSource<RpcResult>> _rpcCallbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<RpcResult>>();

        public RabbitMqEventBusProvider(ISerializerProvider serializerProvider,
            IServiceProvider serviceProvider,
            RabbitMqClientProvider rabbitMqClientProvider,
            RabbitMqClientProvider rabbitMqClientProviderRead,
            ILogger<RabbitMqEventBusProvider> logger)
        {
            _serializerProvider = serializerProvider;
            _rabbitMqClientProvider = rabbitMqClientProvider;
            _rabbitMqClientProviderRead = rabbitMqClientProviderRead;
            _config = _rabbitMqClientProvider.Config;
            _logger = logger;
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="message">数据</param>
        /// <param name="broadcast">是否广播模式（注：对内存队列和redis无效）</param>
        /// <returns></returns>
        public async Task PublishAsync<T>(string key, EventMessageModel<T> message, bool broadcast = false)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            //附加 http header
            Common.AppendHttpHeader(message, ServiceProvider);

            //广播模式
            if (broadcast)
            {
                await PublishBroadcastAsync(key, message);
                return;
            }

            var channel = _rabbitMqClientProvider.GetChannel();
            channel.ConfirmSelect();

            //持久化
            var properties = channel.BasicProperties();

            channel.ExchangeDeclare(
                exchange: _config.ExchangeName,
                durable: true,
                type: "topic",
                autoDelete: false);

            message.Key = key;
            var body = _serializerProvider.SerializeBytes(message);

            channel.BasicPublish(
                exchange: _config.ExchangeName,
                routingKey: key,
                basicProperties: properties,
                body: body);

            channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

            _logger.LogDebug($"RabbitMQ topic message [{key}] has been published. message:{_serializerProvider.Serialize(message)}");

            _rabbitMqClientProvider.ReturnChannel(channel);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 发布事件 数据放到队列，并发布通知到订阅者
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="message">数据集合</param>
        /// <returns></returns>
        public async Task PublishQueueAsync<T>(string key, List<T> message)
        {
            if (message == null)
                return;

            await PushToQueueAsync(key, message);
            await PublishAsync(key, new EventMessageModel<T>(), true);
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="handler">订阅处理</param>
        /// <param name="broadcast">是否广播模式（注：对内存队列和redis无效）</param>
        public void Subscribe<T>(string key, Action<EventMessageModel<T>> handler, bool broadcast = false)
        {
            if (broadcast)
                SubscribeBroadcastInternal(key, handler, false);
            else
                SubscribeInternal(key, handler, false);
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="handler">订阅处理</param>
        /// <param name="broadcast">是否广播模式（注：对内存队列和redis无效）</param>
        public void Subscribe<T>(string key, Func<EventMessageModel<T>, Task> handler, bool broadcast = false)
        {
            if (broadcast)
                SubscribeBroadcastInternal(key, handler, false);
            else
                SubscribeInternal(key, handler, false);
        }

        /// <summary>
        /// 获取某个频道队列数据量
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public int GetQueueLength(string key)
        {
            IModel channel = null;
            try
            {
                var queueName = GetChannelQueueKey(key);
                channel = _rabbitMqClientProvider.GetChannel();
                return (int)channel.MessageCount(queueName);
            }
            catch
            {
                return 0;
            }
            finally
            {
                _rabbitMqClientProvider.ReturnChannel(channel);
            }
        }

        /// <summary>
        /// 获取某个频道队列数据
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="length">获取指定的数据条数</param>
        /// <returns></returns>
        public List<T> GetQueueItems<T>(string key, int length)
        {
            lock (GetLockObject(key))
            {
                var queueName = GetChannelQueueKey(key);
                return GetQueueItemsMethod<T>(queueName, length);
            }
        }

        /// <summary>
        /// 获取某个频道队列数据
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="length">获取指定的数据条数</param>
        /// <returns></returns>
        public async Task<List<T>> GetQueueItemsAsync<T>(string key, int length)
        {
            using (await GetLockObject(key).LockAsync())
            {
                var queueName = GetChannelQueueKey(key);
                return await Task.FromResult(GetQueueItemsMethod<T>(queueName, length));
            }
        }

        /// <summary>
        /// 获取某个频道错误队列数据量
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public int GetErrorQueueLength(string key)
        {
            IModel channel = null;
            try
            {
                var queueName = GetChannelErrorQueueKey(key);
                channel = _rabbitMqClientProvider.GetChannel();
                return (int)channel.MessageCount(queueName);
            }
            catch
            {
                return 0;
            }
            finally
            {
                _rabbitMqClientProvider.ReturnChannel(channel);
            }
        }

        /// <summary>
        /// 获取某个频道错误队列数据
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="length">获取指定的数据条数</param>
        /// <returns></returns>
        public List<T> GetErrorQueueItems<T>(string key, int length)
        {
            lock (GetLockObject(key))
            {
                var queueName = GetChannelErrorQueueKey(key);
                return GetQueueItemsMethod<T>(queueName, length);
            }
        }

        /// <summary>
        /// 获取某个频道错误队列数据
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="length">获取指定的数据条数</param>
        /// <returns></returns>
        public async Task<List<T>> GetErrorQueueItemsAsync<T>(string key, int length)
        {
            using (await GetLockObject(key).LockAsync())
            {
                var queueName = GetChannelErrorQueueKey(key);
                return await Task.FromResult(GetErrorQueueItems<T>(queueName, length));
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="key"></param>
        public void UnSubscribe(string key)
        {
            var channel = _rabbitMqClientProvider.Channel;
            var queue = GetTempChannelQueueName(key);
            var exchange = _config.ExchangeName;
            channel.QueueUnbind(queue, exchange, key);
        }

        /// <summary>
        /// 设置发布订阅是否开启
        /// </summary>
        /// <param name="enable">true 开启开关，false 关闭开关</param>
        /// <param name="key">为空时表示总开关</param>
        public void SetEnable(bool enable, string key = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Enable = enable;
                return;
            }

            _channelEnableDictionary.AddOrUpdate(key, d => enable, (k, value) => enable);
        }

        //#region Test

        ///// <summary>
        ///// 订阅事件 用于单元测试
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key">Key</param>
        ///// <param name="handler">订阅处理</param>
        //public void SubscribeTest<T>(string key, Action<EventMessageModel<T>> handler)
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// 订阅事件 用于单元测试
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key">Key</param>
        ///// <param name="handler">订阅处理</param>
        //public async Task SubscribeTest<T>(string key, Func<EventMessageModel<T>, Task> handler)
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// 订阅事件 从队列读取数据 用于单元测试
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key">Key</param>
        ///// <param name="handler">订阅处理</param>
        //public void SubscribeQueueTest<T>(string key, Action<Func<int, List<T>>> handler)
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// 订阅事件 从队列读取数据 用于单元测试
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key">Key</param>
        ///// <param name="handler">订阅处理</param>
        //public async Task SubscribeQueueTest<T>(string key, Func<Func<int, Task<List<T>>>, Task> handler)
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// 订阅事件 从队列读取数据 分批次消费 用于单元测试
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key">Key</param>
        ///// <param name="length">每次处理条数</param>
        ///// <param name="delay">每次处理间隔 毫秒</param>
        ///// <param name="exceptionHandler">异常处理方式</param>
        ///// <param name="handler">订阅处理</param>
        ///// <param name="error">发生异常时回调</param>
        ///// <param name="completed">本次消费完成回调 最后执行</param>
        //public async Task SubscribeQueueTest<T>(string key, int length, int delay, ExceptionHandlerEnum exceptionHandler, Func<List<T>, Task> handler,
        //    Func<Exception, List<T>, Task> error = null, Func<Task> completed = null)
        //{
        //    throw new NotImplementedException();
        //}

        //#endregion

        public void Dispose()
        {
            _rabbitMqClientProvider.Dispose();
            _rabbitMqClientProviderRead.Dispose();
        }

        #region private

        private void SubscribeInternal<T>(string key, Action<EventMessageModel<T>> handler, bool checkEnable = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _logger.LogWarning($"{DateTime.Now} RabbitMQ 开始订阅 " +
                               $"[{_config.ExchangeName}] " +
                               $"[{key}] " +
                               $"[PrefetchSize：{_config.PrefetchSize}]" +
                               $"[PrefetchCount：{_config.PrefetchCount}]" +
                               $"......");

            var channel = _rabbitMqClientProvider.Channel;

            channel.ExchangeDeclare(
                exchange: _config.ExchangeName,
                durable: true,
                type: "topic",
                autoDelete: false);

            //队列
            var queueName = GetTempChannelQueueName(key);
            channel.QueueDeclare(queueName, true, false, false, null);

            channel.QueueBind(
                queue: queueName,
                exchange: _config.ExchangeName,
                routingKey: key);

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

        private void SubscribeInternal<T>(string key, Func<EventMessageModel<T>, Task> handler, bool checkEnable = true)
        {
            //Task.Factory.StartNew(() =>
            //{
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _logger.LogWarning($"{DateTime.Now} RabbitMQ 开始订阅 " +
                               $"[{_config.ExchangeName}] " +
                               $"[{key}] " +
                               $"[PrefetchSize：{_config.PrefetchSize}]" +
                               $"[PrefetchCount：{_config.PrefetchCount}]" +
                               $"......");

            var channel = _rabbitMqClientProvider.Channel;

            channel.ExchangeDeclare(
                exchange: _config.ExchangeName,
                durable: true,
                type: "topic",
                autoDelete: false);

            //队列
            var queueName = GetTempChannelQueueName(key);
            channel.QueueDeclare(queueName, true, false, false, null);

            channel.QueueBind(
                queue: queueName,
                exchange: _config.ExchangeName,
                routingKey: key);

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
            //}, TaskCreationOptions.LongRunning);
        }

        private string GetChannelQueueKey(string key)
        {
            return _config.DataQueuePrefixKey + key;
        }

        private string GetChannelErrorQueueKey(string channel)
        {
            return _config.DataErrorQueuePrefixKey + channel;
        }

        private string GetTempChannelQueueName(string key)
        {
            if (_queueNameDictionary.TryGetValue(key, out string name))
            {
                return name;
            }

            name = _config.SampleQueuePrefixKey + key;

            _queueNameDictionary.TryAdd(key, name);

            return name;
        }

        private bool IsEnable(string key)
        {
            return Enable && (!_channelEnableDictionary.TryGetValue(key, out bool enable) || enable);
        }

        /// <summary>
        /// 获取某个频道队列数据
        /// </summary>
        /// <param name="queueName">queueName</param>
        /// <param name="length">获取指定的数据条数</param>
        /// <returns></returns>
        private List<T> GetQueueItemsMethod<T>(string queueName, int length)
        {
            var channel = _rabbitMqClientProviderRead.GetChannel();

            var list = new List<T>();

            while (list.Count < length)
            {
                if (channel.IsClosed)
                    channel = _rabbitMqClientProviderRead.GetChannel();

                try
                {
                    var result = channel.BasicGet(queueName, false);
                    if (result == null)
                        break;

                    try
                    {
                        list.Add(_serializerProvider.Deserialize<T>(result.Body.ToArray()));
                        channel.BasicAck(result.DeliveryTag, false);
                    }
                    catch
                    {
                        //PushToQueueAsync("abcvfdrerer", new List<int>() { 1 }).GetAwaiter().GetResult();
                        //Console.WriteLine("-------------------------------------------------------------------------------get new channel");
                        channel.BasicReject(result.DeliveryTag, true);
                        Thread.Sleep(500);
                    }

                    if (result.MessageCount <= 0)
                        break;
                }
                catch (OperationInterruptedException ex)
                {
                    //队列不存在
                    if (ex.ShutdownReason?.ReplyCode == 404)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    channel = _rabbitMqClientProviderRead.GetChannel();
                    continue;
                }
            }

            _rabbitMqClientProviderRead.ReturnChannel(channel);
            return list;
        }

        //private int GetTotalPagesFromQueue(string channel, int length)
        //{
        //    var total = GetQueueLength(channel);

        //    return total <= 0 ? 0 : (total / length + (total % length > 0 ? 1 : 0));
        //}

        private AsyncLock GetLockObject(string key)
        {
            if (_lockObjectDictionary.TryGetValue(key, out AsyncLock obj))
                return obj;

            return new AsyncLock();
        }

        private EventMessageModel<T> GetResultFromBody<T>(string key, BasicDeliverEventArgs ea, IServiceProvider serviceProvider)
        {
            var data = _serializerProvider.Deserialize<EventMessageModel<T>>(ea.Body.ToArray());
            if (data != null)
            {
                data.ScopeServiceProvider = serviceProvider;

                //处理 Http Header
                Common.MergeHttpHeaderToHttpContext(serviceProvider, data);
            }

            _logger.LogDebug($"{DateTime.Now}：频道【{key}】 收到消息： {Encoding.Default.GetString(ea.Body.ToArray())}");

            return data;
        }

        #endregion
    }
}