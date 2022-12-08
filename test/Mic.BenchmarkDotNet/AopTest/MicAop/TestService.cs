using Mic.Aop;
using Mic.Aop.TestWeb.AopSpace;
using Mic.BenchmarkDotNet.AopTest.RougamoAop;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mic.BenchmarkDotNet.AopTest.MicAop
{
    //[Log(Index = 1)]
    public interface ITestService
    {
        [Cache(Key = "GetDateTimeSync", Seconds = 3600)]
        DateTime GetDateTimeSync();

        DateTime GetDateTimeSyncDirect();



        [Cache(Key = "GetDateTimeAsync", Seconds = 3600)]
        ValueTask<DateTime> GetDateTimeAsync();

        ValueTask<DateTime> GetDateTimeAsyncDirect();







    }

    public class TestService// : ITestService
    {
        public TestService()
        {
        }

        [RoigamoCache]
        public virtual DateTime GetDateTimeSync()
        {
            return DateTime.Now;
        }

        public virtual DateTime GetDateTimeSyncDirect()
        {
            return DateTime.Now;
        }

        [RoigamoCache]
        public virtual async ValueTask<DateTime> GetDateTimeAsync()
        {
            await ValueTask.CompletedTask;
            return DateTime.Now;
        }

        public virtual async ValueTask<DateTime> GetDateTimeAsyncDirect()
        {
            await ValueTask.CompletedTask;
            return DateTime.Now;
        }

        public virtual DateTime SampleSync()
        {
            return DateTime.Now;
        }

        public virtual async ValueTask<DateTime> SampleAsync()
        {
            await ValueTask.CompletedTask;
            return DateTime.Now;
        }
    }

    public sealed class TestService_Aop : TestService
    {
        private readonly IServiceProvider _serviceProvider0;
        public TestService_Aop(IServiceProvider serviceProvider0)
        {
            _serviceProvider0 = serviceProvider0;
        }

        public override DateTime GetDateTimeSync()
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { },
                false,
                true,
                null);

            var aopInterceptor0 = _serviceProvider0.GetRequiredService<CacheAttribute>();
            aopInterceptor0.Key = "GetDateTimeSync";
            aopInterceptor0.Seconds = 3600;
            if (aopInterceptor0.HasBefore) aopContext = aopInterceptor0.Before(aopContext);
            if (aopInterceptor0.HasAopNext)
            {
                if (aopInterceptor0.HasActualNext)
                {
                    aopContext.ActualMethod = () => base.GetDateTimeSync();
                }
                aopContext = aopInterceptor0.Next(aopContext);
            }
            else
            {
                if (aopInterceptor0.HasActualNext)
                {
                    aopContext.ReturnValue = base.GetDateTimeSync();
                }
            }
            if (aopInterceptor0.HasAfter) aopContext = aopInterceptor0.After(aopContext);

            return aopContext.ReturnValue;
        }

        public override async ValueTask<DateTime> GetDateTimeAsync()
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { },
                true,
                true,
                null);

            var aopInterceptor0 = _serviceProvider0.GetRequiredService<CacheAttribute>();
            aopInterceptor0.Key = "GetDateTimeAsync";
            aopInterceptor0.Seconds = 3600;
            if (aopInterceptor0.HasBefore) aopContext = await aopInterceptor0.BeforeAsync(aopContext);
            if (aopInterceptor0.HasAopNext)
            {
                if (aopInterceptor0.HasActualNext)
                {
                    aopContext.ActualMethod = () => base.GetDateTimeAsync();
                }
                aopContext = await aopInterceptor0.NextAsync(aopContext);
            }
            else
            {
                if (aopInterceptor0.HasActualNext)
                {
                    aopContext.ReturnValue = await base.GetDateTimeAsync();
                }
            }
            if (aopInterceptor0.HasAfter) aopContext = await aopInterceptor0.AfterAsync(aopContext);

            return aopContext.ReturnValue;
        }

        public override DateTime SampleSync()
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { },
                false,
                true,
                null);

            var aopInterceptor0 = _serviceProvider0.GetRequiredService<SampleAttribute>();
            if (aopInterceptor0.HasBefore) aopContext = aopInterceptor0.Before(aopContext);
            if (aopInterceptor0.HasAopNext)
            {
                if (aopInterceptor0.HasActualNext)
                {
                    aopContext.ActualMethod = () => base.SampleSync();
                }
                aopContext = aopInterceptor0.Next(aopContext);
            }
            else
            {
                if (aopInterceptor0.HasActualNext)
                {
                    aopContext.ReturnValue = base.SampleSync();
                }
            }
            if (aopInterceptor0.HasAfter) aopContext = aopInterceptor0.After(aopContext);

            return aopContext.ReturnValue;
        }

        public override async ValueTask<DateTime> SampleAsync()
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { },
                true,
                true,
                null);

            var aopInterceptor0 = _serviceProvider0.GetRequiredService<SampleAttribute>();
            if (aopInterceptor0.HasBefore) aopContext = await aopInterceptor0.BeforeAsync(aopContext);
            if (aopInterceptor0.HasAopNext)
            {
                if (aopInterceptor0.HasActualNext)
                {
                    aopContext.ActualMethod = () => base.SampleAsync();
                }
                aopContext = await aopInterceptor0.NextAsync(aopContext);
            }
            else
            {
                if (aopInterceptor0.HasActualNext)
                {
                    aopContext.ReturnValue = await base.SampleAsync();
                }
            }
            if (aopInterceptor0.HasAfter) aopContext = await aopInterceptor0.AfterAsync(aopContext);

            return aopContext.ReturnValue;
        }

    }
}
