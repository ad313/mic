using Rougamo.Context;
using Rougamo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Mic.BenchmarkDotNet.AopTest.RougamoAop
{
    //public class TestService// : ITestService
    //{
    //    public TestService()
    //    {
    //    }


    //    public virtual DateTime GetDateTimeSync()
    //    {
    //        return DateTime.Now;
    //    }

    //    public virtual DateTime GetDateTimeSyncDirect()
    //    {
    //        return DateTime.Now;
    //    }

    //    public virtual async ValueTask<DateTime> GetDateTimeAsync()
    //    {
    //        await ValueTask.CompletedTask;
    //        return DateTime.Now;
    //    }

    //    public virtual async ValueTask<DateTime> GetDateTimeAsyncDirect()
    //    {
    //        await ValueTask.CompletedTask;
    //        return DateTime.Now;
    //    }
    //}

    class RoigamoCacheAttribute : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            //Console.WriteLine($"{context.Method.Name} {nameof(OnEntry)}");
        }

        public override void OnExit(MethodContext context)
        {
            //Console.WriteLine($"{context.Method.Name} {nameof(OnExit)} - ms");
        }
    }
}
