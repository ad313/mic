using System;
using System.Threading.Tasks;
using Mic.Aop.TestWeb.AopSpace;

namespace Mic.Aop.TestAspnetcore3
{
    //[IgnoreAop]
    public class SampleService2 
    {
        /// <summary>
        /// hahah
        /// </summary>
        public string Name { get; set; }

        public SampleService2()
        {
        }

        [Log2]
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
            await Task.CompletedTask;
            return DateTime.Now;
        }

        public virtual async ValueTask<DateTime> GetDateTimeAsyncDirect()
        {
            await Task.CompletedTask;
            return DateTime.Now;
        }

        public virtual DateTime SampleSync(string aaa)
        {
            return DateTime.Now;
        }

        public virtual async ValueTask<DateTime> SampleAsync()
        {
            await Task.CompletedTask;
            return DateTime.Now;
        }
    }
}
