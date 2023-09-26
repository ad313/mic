using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mic.Aop.TestWeb.AopSpace
{
    //[IgnoreAop]
    public interface ITestService
    {
        [Cache(Key = "GetDateTimeSync", Seconds = 3600)]
        DateTime GetDateTimeSync();
        
        DateTime GetDateTimeSyncDirect();
        



        [Cache(Key = "GetDateTimeAsync", Seconds = 3600)]
        ValueTask<DateTime> GetDateTimeAsync();

        ValueTask<DateTime> GetDateTimeAsyncDirect();

        [Sample]
        DateTime SampleSync(string aaa);

        [Sample]
        ValueTask<DateTime> SampleAsync();

    }

    //[IgnoreAop]
    public class TestService : ITestService
    {
        /// <summary>
        /// hahah
        /// </summary>
        [Cache]
        public string Name { get; set; }

        public TestService()
        {
        }


        public virtual DateTime GetDateTimeSync()
        {
            return DateTime.Now;
        }

        public virtual DateTime GetDateTimeSyncDirect()
        {
            return DateTime.Now;
        }

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

        public virtual DateTime SampleSync(string aaa)
        {
            return DateTime.Now;
        }

        public virtual async ValueTask<DateTime> SampleAsync()
        {
            await ValueTask.CompletedTask;
            return DateTime.Now;
        }
    }

    public class TestService2
    {
        [Log]
        [Cache(Key = "aaa")]
        public virtual DateTime GetDateTimeSync(string a)
        {
            return DateTime.Now;
        }

        
    }
}
