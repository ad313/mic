using Mic.Aop;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Mic.BenchmarkDotNet.AopTest.MicAop
{
    /// <summary>
    /// 缓存
    /// </summary>
    public class CacheAttribute : AopInterceptor
    {
        public string Key { get; set; }

        public long Seconds { get; set; }

        private IMemoryCache _memoryCache;

        /// <summary>
        /// 缓存服务
        /// </summary>
        public CacheAttribute()
        {
            HasAopNext = false;
            HasActualNext = false;
            HasAfter = false;
        }

        public override AopContext Before(AopContext context)
        {
            //Console.WriteLine("cache Attribute Before");

            _memoryCache = context.ServiceProvider.GetService<IMemoryCache>();

            var cache = _memoryCache.Get<DateTime?>(Key);
            if (cache != null)
            {
                context.ReturnValue = cache.Value;
                HasAopNext = false;
                HasActualNext = false;
            }
            else
            {
                HasAopNext = true;
                HasActualNext = true;
            }

            return context;
        }

        /// <summary>执行前操作，异步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValueTask<AopContext> BeforeAsync(AopContext context)
        {
            //Console.WriteLine("cache Attribute BeforeAsync");

            _memoryCache = context.ServiceProvider.GetService<IMemoryCache>();
            var cache = _memoryCache.Get<DateTime?>(Key);
            if (cache != null)
            {
                context.ReturnValue = cache.Value;
                HasAopNext = false;
                HasActualNext = false;
            }
            else
            {
                HasAopNext = true;
                HasActualNext = true;
            }

            return ValueTask.FromResult(context);
        }

        public override AopContext Next(AopContext context)
        {
            //Console.WriteLine("cache Attribute Next");

            context = base.Next(context);

            //加入缓存
            _memoryCache.Set(Key, (object)context.ReturnValue, DateTimeOffset.Now.AddSeconds(Seconds));

            return context;
        }

        public override async ValueTask<AopContext> NextAsync(AopContext context)
        {
            //Console.WriteLine("cache Attribute NextAsync");

            context = await base.NextAsync(context);

            //加入缓存
            _memoryCache.Set(Key, (object)context.ReturnValue, DateTimeOffset.Now.AddSeconds(Seconds));

            return context;
        }
    }
}
