using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mic.Aop.TestWeb.AopSpace
{
    /// <summary>
    /// 常规服务，执行所有方法
    /// </summary>
    public class SampleAttribute : AopInterceptor
    {
        /// <summary>执行前操作，同步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override AopContext Before(AopContext context)
        {
            return base.Before(context);
        }

        /// <summary>执行前操作，异步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValueTask<AopContext> BeforeAsync(AopContext context)
        {
            return base.BeforeAsync(context);
        }

        public override AopContext After(AopContext context)
        {
            //Console.WriteLine("log trace sync");
            return context;
        }
        
        /// <summary>执行后操作，异步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValueTask<AopContext> AfterAsync(AopContext context)
        {
            //Console.WriteLine("log trace async");
            return base.AfterAsync(context);
        }

        /// <summary>执行方法，同步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override AopContext Next(AopContext context)
        {
            return base.Next(context);
        }

        /// <summary>执行方法，异步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValueTask<AopContext> NextAsync(AopContext context)
        {
            return base.NextAsync(context);
        }
    }
}
