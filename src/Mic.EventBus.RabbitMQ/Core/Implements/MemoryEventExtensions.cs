using Mic.EventBus.RabbitMQ.Core.Abstractions;
using Mic.EventBus.RabbitMQ.Core.Implements;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 注册 RabbitMQ EventBus
    /// </summary>
    public static partial class MemoryEventExtensions
    {
        /// <summary>
        /// 注册 内存队列 IMemoryEventBusProvider
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static void AddMemoryEventBus(this IServiceCollection service)
        {
            service.AddSingleton<ISerializerProvider, SerializerProvider>();
            service.AddSingleton<IMemoryEventBusProvider, MemoryEventBusProviderStandard>();
        }
    }
}