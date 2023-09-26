using Mic.LazyLoader;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LazyLoaderExtensions
    {
        public static IServiceCollection AddLazyLoader(this IServiceCollection services)
        {
            services.AddTransient(typeof(Lazy<>), typeof(LazyLoader<>));
            return services;
        }
    }
}