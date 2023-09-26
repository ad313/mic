using System;
using System.Threading.Tasks;

namespace Mic.Aop
{
    public class AopInterceptor : Attribute, IAopInterceptor
    {
        /// <summary>
        /// 是否执行 Before，默认 true
        /// </summary>
        public bool HasBefore { get; set; }

        /// <summary>
        /// 是否执行 After，默认 true
        /// </summary>
        public bool HasAfter { get; set; }

        /// <summary>
        /// 是否执行 Aop 的 Next，默认 true
        /// </summary>
        public bool HasAopNext { get; set; }

        /// <summary>
        /// 是否执行实际的方法，默认 true
        /// </summary>
        public bool HasActualNext { get; set; }

        /// <summary>
        /// AopTag
        /// </summary>
        public bool AopTag { get; set; }

        public AopInterceptor()
        {
            HasBefore = true;
            HasAopNext = true;
            HasActualNext = true;
            HasAfter = true;
        }

        public virtual AopContext Before(AopContext context) => context;

        public virtual async ValueTask<AopContext> BeforeAsync(AopContext context)
        {
            await ValueTask.CompletedTask;
            return context;
        }

        public virtual AopContext After(AopContext context)
        {
            return context.Exception != null ? throw context.Exception : context;
        }

        public virtual async ValueTask<AopContext> AfterAsync(AopContext context)
        {
            if (context.Exception != null)
                throw context.Exception;

            await ValueTask.CompletedTask;
            return context;
        }

        public virtual AopContext Next(AopContext context)
        {
            try
            {
                context.Invoke();
            }
            catch (Exception e)
            {
                context.Exception = e;
            }
            return context;
        }

        public virtual async ValueTask<AopContext> NextAsync(AopContext context)
        {
            try
            {
                context = await context.InvokeAsync();
            }
            catch (Exception e)
            {
                context.Exception = e;
            }

            return context;
        }

        public virtual void Clear()
        {

        }
    }
}
