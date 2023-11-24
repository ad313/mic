using Mic.EventBus.RabbitMQ;
using Mic.EventBus.RabbitMQ.Core.Abstractions;
using Mic.EventBus.RabbitMQ.Core.Implements;
using Mic.EventBus.RabbitMQ.Runtime;
using Microsoft.AspNetCore.Http;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 注册 RabbitMQ EventBus
    /// </summary>
    public static partial class RabbitMqEventExtensions
    {
        /// <summary>
        /// 注册 RabbitMQ EventBus
        /// </summary>
        /// <param name="service"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static void AddEventBusUseRabbitMq(this IServiceCollection service, Action<RabbitMqConfig> option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            var config = new RabbitMqConfig()
            {
                VirtualHost = "/",
                PrefetchSize = 0,
                PrefetchCount = 1
            };
            option.Invoke(config);
            config.Check();

            service.AddSingleton(config);
            service.AddSingleton<ISerializerProvider, SerializerProvider>();
            service.AddTransient<RabbitMqClientProvider>();
            service.AddSingleton<IEventBusProvider, RabbitMqEventBusProvider>();
            service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //init rpc
            new DependencyRegister().RegisterServices();
            service.AddHostedService<RabbitMqInitHost>();
        }

        /// <summary>
        /// 注册 RabbitMQ EventBus
        /// </summary>
        /// <param name="service"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static void AddEventBusUseRabbitMq(this IServiceCollection service, RabbitMqConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config.Check();

            service.AddSingleton(config);
            service.AddSingleton<ISerializerProvider, SerializerProvider>();
            service.AddTransient<RabbitMqClientProvider>();
            service.AddSingleton<IEventBusProvider, RabbitMqEventBusProvider>();
            service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //init rpc
            new DependencyRegister().RegisterServices();
            service.AddHostedService<RabbitMqInitHost>();
        }
    }
}