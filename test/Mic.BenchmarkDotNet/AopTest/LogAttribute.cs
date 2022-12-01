using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mic.Aop.TestWeb.AopSpace
{
    /// <summary>
    /// 字典服务
    /// </summary>
    public class LogAttribute : AopInterceptor
    {
        public int Index { get; set; }
        
        public override AopContext Before(AopContext context)
        {
            //Console.WriteLine("Before LogAttribute");
            return context;
        }

        public override AopContext After(AopContext context)
        {
            //Console.WriteLine("After LogAttribute");
            return context;
        }

        /// <summary>执行前操作，异步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValueTask<AopContext> BeforeAsync(AopContext context)
        {
            //Console.WriteLine("BeforeAsync LogAttribute");
            return base.BeforeAsync(context);
        }

        /// <summary>执行后操作，异步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValueTask<AopContext> AfterAsync(AopContext context)
        {
            //Console.WriteLine("AfterAsync LogAttribute");
            return base.AfterAsync(context);
        }

        public override AopContext Next(AopContext context)
        {
            //context.SetResultContext(new ResultContext()
            //{
            //    HasReturnValue = true,
            //    ReturnValue = "hahahhaha，被拦截了"
            //});

            //return context;

            //Console.WriteLine("next LogAttribute");

            return base.Next(context);
        }

        public override ValueTask<AopContext> NextAsync(AopContext context)
        {
            //context.SetResultContext(new ResultContext()
            //{
            //    IsTask = false,
            //    HasReturnValue = true,
            //    ReturnValue = "hahahhaha，被拦截了"
            //});

            ////Console.WriteLine("Base method Async");

            //return Task.FromResult(context);

            //Console.WriteLine("next LogAttribute");

            return base.NextAsync(context);
        }
    }

    public class Log2Attribute : AopInterceptor
    {
        public EnumParam Pa { get; set; }

        public int  Index { get; set; }

        public override AopContext Before(AopContext context)
        {
            //Console.WriteLine("Before Log22222Attribute");
            return context;
        }

        public override AopContext After(AopContext context)
        {
            //Console.WriteLine("After Log22222Attribute");
            return context;
        }

        /// <summary>执行前操作，异步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValueTask<AopContext> BeforeAsync(AopContext context)
        {
            //Console.WriteLine("BeforeAsync Log22222Attribute");
            return base.BeforeAsync(context);
        }

        /// <summary>执行后操作，异步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValueTask<AopContext> AfterAsync(AopContext context)
        {
            //Console.WriteLine("AfterAsync Log22222Attribute");
            return base.AfterAsync(context);
        }

        public override AopContext Next(AopContext context)
        {
            //context.SetResultContext(new ResultContext()
            //{
            //    HasReturnValue = true,
            //    ReturnValue = "hahahhaha，被拦截了"
            //});

            //return context;

            //Console.WriteLine("next Log22222Attribute");

            return base.Next(context);
        }

        public override ValueTask<AopContext> NextAsync(AopContext context)
        {
            //context.SetResultContext(new ResultContext()
            //{
            //    IsTask = false,
            //    HasReturnValue = true,
            //    ReturnValue = "hahahhaha，被拦截了"
            //});

            ////Console.WriteLine("Base method Async");

            //return Task.FromResult(context);

            //Console.WriteLine("next Log22222Attribute");

            return base.NextAsync(context);
        }
    }


    public class Log3Attribute : AopInterceptor
    {
        public EnumParam Pa { get; set; }

        public override AopContext Before(AopContext context)
        {
            //Console.WriteLine("Before Log3333Attribute");
            return context;
        }

        public override AopContext After(AopContext context)
        {
            //Console.WriteLine("After Log3333Attribute");
            return context;
        }

        /// <summary>执行前操作，异步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValueTask<AopContext> BeforeAsync(AopContext context)
        {
            //Console.WriteLine("BeforeAsync Log3333Attribute");
            return base.BeforeAsync(context);
        }

        /// <summary>执行后操作，异步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValueTask<AopContext> AfterAsync(AopContext context)
        {
            //Console.WriteLine("AfterAsync Log3333Attribute");
            return base.AfterAsync(context);
        }

        public override AopContext Next(AopContext context)
        {
            //context.SetResultContext(new ResultContext()
            //{
            //    HasReturnValue = true,
            //    ReturnValue = "hahahhaha，被拦截了"
            //});

            //return context;

            //Console.WriteLine("next Log3333Attribute");

            return base.Next(context);
        }

        public override ValueTask<AopContext> NextAsync(AopContext context)
        {
            //context.SetResultContext(new ResultContext()
            //{
            //    IsTask = false,
            //    HasReturnValue = true,
            //    ReturnValue = "hahahhaha，被拦截了"
            //});

            ////Console.WriteLine("Base method Async");

            //return Task.FromResult(context);

            //Console.WriteLine("next Log3333Attribute");

            return base.NextAsync(context);
        }
    }

    public enum EnumParam
    {
        One,
        Two, 
        Three
    }

    
}
