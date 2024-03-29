﻿using System;

namespace Mic.EventBus.RabbitMQ
{
    public class RabbitMqConfig
    {
        /// <summary>
        /// 交换机
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// HostName
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 账户
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 虚拟主机
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// prefetchSize
        /// </summary>
        public uint PrefetchSize { get; set; } = 0;

        /// <summary>
        /// prefetchCount
        /// </summary>
        public ushort PrefetchCount { get; set; } = 1;

        /// <summary>
        /// 数据队列key前缀
        /// </summary>
        public string DataQueuePrefixKey { get; private set; } = "mic.event.queue.data.";

        /// <summary>
        /// 数据错误队列key前缀
        /// </summary>
        public string DataErrorQueuePrefixKey { get; private set; } = "mic.event.queue.data.error.";

        /// <summary>
        /// 普通队列key前缀
        /// </summary>
        public string SampleQueuePrefixKey { get; private set; } = "mic.event.queue.sample.";

        /// <summary>
        /// 死信队列 交换机
        /// </summary>
        public string DeadLetterExchange { get; private set; } = "mic.event_dead_letter_exchange";

        /// <summary>
        /// 死信队列 路由key前缀
        /// </summary>
        public string DeadLetterPrefixKey { get; private set; } = "mic.event.dead.letter.route.key_";

        /// <summary>
        /// 死信队列 宿主队列key前缀
        /// </summary>
        public string DeadLetterHostQueuePrefixKey { get; private set; } = "mic.event.dead.letter.queue.host_";

        /// <summary>
        /// 死信队列 消费队列key前缀
        /// </summary>
        public string DeadLetterWorkQueuePrefixKey { get; private set; } = "mic.event.dead.letter.queue.work_";

        /// <summary>
        /// Rpc服务端队列前缀
        /// </summary>
        public string RpcServerQueuePrefixKey { get; private set; } = "mic.event.rpc.server.queue_";

        /// <summary>
        /// Rpc客户端端队列前缀
        /// </summary>
        public string RpcClientQueuePrefixKey { get; private set; } = "mic.event.rpc.client.queue_";

        public void Check()
        {
            if (string.IsNullOrWhiteSpace(ExchangeName))
                throw new ArgumentNullException(nameof(ExchangeName));

            if (string.IsNullOrWhiteSpace(HostName))
                throw new ArgumentNullException(nameof(HostName));

            if (string.IsNullOrWhiteSpace(UserName))
                throw new ArgumentNullException(nameof(UserName));

            if (string.IsNullOrWhiteSpace(Password))
                throw new ArgumentNullException(nameof(Password));

            if (Port <= 0)
                throw new ArgumentException(nameof(Port));
        }

        public string GetDeadLetterRouteKey(string key)
        {
            return DeadLetterPrefixKey + key;
        }

        public string GetDeadLetterHostQueueKey(string key, long seconds)
        {
            return DeadLetterHostQueuePrefixKey + $"{seconds}_" + key;
        }

        public string GetDeadLetterWorkQueueKey(string key)
        {
            return DeadLetterWorkQueuePrefixKey + key;
        }

        public string GetRpcServerQueueKey(string key)
        {
            return RpcServerQueuePrefixKey + key;
        }

        public string GetRpcClientQueueKey(string key)
        {
            return RpcClientQueuePrefixKey + key;
        }
    }
}