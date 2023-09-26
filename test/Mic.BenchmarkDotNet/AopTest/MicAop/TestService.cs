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

    
}
