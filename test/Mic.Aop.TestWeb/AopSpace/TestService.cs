using System;
using System.Threading.Tasks;

namespace Mic.Aop.TestWeb.AopSpace
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

        [Sample]
        DateTime SampleSync();

        [Sample]
        ValueTask<DateTime> SampleAsync();

    }
    
    public class TestService : ITestService
    {
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

    public class TestService2
    {
        [Cache(Key = "aaa")]
        public virtual DateTime GetDateTimeSync()
        {
            return DateTime.Now;
        }

        
    }
}
