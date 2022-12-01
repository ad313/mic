using System.Threading.Tasks;
using System;

namespace Mic.Aop.TestWeb.AopSpace
{
    /// <summary>
    /// 缓存
    /// </summary>
    public class CacheAttribute : AopInterceptor
    {
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

            if (DateTime.Now.Second % 2 == 0)
            {
                context.ReturnValue = DateTime.Now.ToString();
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

            if (DateTime.Now.Second % 2 == 0)
            {
                context.ReturnValue = DateTime.Now.ToString();
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

            var realValue = context.ReturnValue;

            //加入缓存

            context.ReturnValue = "实际方法返回的值 - " + context.ReturnValue;

            return context;
        }

        public override async ValueTask<AopContext> NextAsync(AopContext context)
        {
            //Console.WriteLine("cache Attribute NextAsync");

            context = await base.NextAsync(context);

            var realValue = context.ReturnValue;

            //加入缓存

            context.ReturnValue = "实际方法返回的值 - " + context.ReturnValue;

            return context;
        }
    }
}
