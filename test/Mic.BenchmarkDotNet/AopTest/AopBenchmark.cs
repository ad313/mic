using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Mic.Aop.TestWeb.AopSpace;
using Mic.BenchmarkDotNet.AopTest.Pool;
using Microsoft.Extensions.DependencyInjection;

//using Mic.BenchmarkDotNet.AopTest.Pool;

namespace Mic.BenchmarkDotNet.AopTest
{
    [MemoryDiagnoser]
    public class AopBenchmark
    {
        readonly TestService directService ;
        readonly TestService_Aop aopService;

        private const int totalCount = 10000;

        public AopBenchmark()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<LogAttribute>();
            services.AddTransient<Log2Attribute>();
            services.AddTransient<Log3Attribute>();

            services.AddTransient<TestService>();
            services.AddTransient<TestService_Aop>();
            var serviceProvider = services.BuildServiceProvider();

            directService = serviceProvider.GetRequiredService<TestService>();
            aopService = serviceProvider.GetRequiredService<TestService_Aop>();

            PollHelper.Init();
        }

        #region Sync

        //[Benchmark(Baseline = true)]
        //public void DirectService_HasReturnSync()
        //{
        //    //var directService = new TestService();
        //    directService.HasReturnSync();
        //}

        //[Benchmark]
        //public void AopService_HasReturnSyncNoNextAndAfterBase()
        //{
        //    //var aopService = new TestService_Aop2(null);
        //    aopService.HasReturnSyncNoNextAndAfterBase();
        //}

        //[Benchmark]
        //public void AopService_HasReturnSyncNoNextAndAfterBasePool()
        //{
        //    //var aopService = new TestService_Aop2(null);
        //    aopService.HasReturnSyncNoNextAndAfterBasePool();
        //}

        //[Benchmark]
        //public void AopService_HasReturnSyncNoNextAndAfterBaseServiceProvider()
        //{
        //    //var aopService = new TestService_Aop2(null);
        //    aopService.HasReturnSyncNoNextAndAfterBasePoolServiceProvider();
        //}

        [Benchmark(Baseline = true)]
        public async Task DirectService_HasReturnAsync()
        {
            //var directService = new TestService();
            await directService.HasReturnAsync("a");
        }

        [Benchmark]
        public async Task AopService_HasReturnAsyncNoNextAndAfterBase()
        {
            //var aopService = new TestService_Aop2(null);
            await aopService.HasReturnAsyncNoNextAndAfterBase("a");
        }

        [Benchmark]
        public async Task AopService_HasReturnAsyncNoNextAndAfterBasePool()
        {
            //var aopService = new TestService_Aop2(null);
            await aopService.HasReturnAsyncNoNextAndAfterBasePool("a");
        }

        [Benchmark]
        public async Task AopService_HasReturnAsyncNoNextAndAfterBaseServiceProvider()
        {
            //var aopService = new TestService_Aop2(null);
            await aopService.HasReturnAsync("a");
        }

        [Benchmark]
        public async Task AopService_HasReturnAsyncWithNextAndAfterBaseServiceProvider()
        {
            //var aopService = new TestService_Aop2(null);
            await aopService.HasReturnAsyncWithNextAndAfterBasePool("a");
        }






        //[Benchmark]
        //public void AopService_HasReturnSync_NoNext()
        //{
        //    //var aopService = new TestService_Aop2(null);
        //    aopService.HasReturnSync_NoNext();
        //}

        //[Benchmark]
        //public async Task AopService_HasReturnAsync()
        //{
        //    //var aopService = new TestService_Aop2(null);
        //    await aopService.HasReturnAsync("aaa");
        //}

        //[Benchmark]
        //public void AopService_HasReturnSync2()
        //{
        //    //var aopService = new TestService_Aop2(null);
        //    aopService.HasReturnSync2();
        //}

        //[Benchmark]
        //public void AopService_HasReturnSync3_NoPool()
        //{
        //    //var aopService = new TestService_Aop2(null);
        //    aopService.HasReturnSync3_NoPool();
        //}

        #endregion



        //#region Async

        //[Benchmark]
        //public async Task DirectService_HasReturnAsync()
        //{
        //    //var directService = new TestService();
        //    await directService.HasReturnAsync("a");
        //}

        //[Benchmark]
        //public async Task AopService_HasReturnAsync()
        //{
        //    //var aopService = new TestService_Aop2(null);
        //    await aopService.HasReturnAsync("a");
        //}

        //#endregion




        //#region Sync 10000
        //[Benchmark]
        //public void DirectService_HasReturnSync_10000()
        //{
        //    for (int i = 0; i < totalCount; i++)
        //    {
        //        directService.HasReturnSync();
        //    }

        //    //var directService = new TestService();

        //}

        //[Benchmark]
        //public void AopService_HasReturnSync_10000()
        //{
        //    for (int i = 0; i < totalCount; i++)
        //    {
        //        aopService.HasReturnSync();
        //    }
        //    //var aopService = new TestService_Aop2(null);

        //}

        //[Benchmark]
        //public void AopService_HasReturnSync3_10000()
        //{
        //    for (int i = 0; i < totalCount; i++)
        //    {
        //        aopService.HasReturnSync2();
        //    }
        //    //var aopService = new TestService_Aop2(null);

        //}

        //[Benchmark]
        //public void AopService_HasReturnSync3_NoPool_10000()
        //{
        //    for (int i = 0; i < totalCount; i++)
        //    {
        //        aopService.HasReturnSync3_NoPool();
        //    }
        //    //var aopService = new TestService_Aop2(null);

        //}
        //#endregion


        //#region Async 10000
        //[Benchmark]
        //public void DirectService_HasReturnAsync_10000()
        //{
        //    for (int i = 0; i < totalCount; i++)
        //    {
        //        directService.HasReturnAsync("a");
        //    }

        //    //var directService = new TestService();

        //}

        //[Benchmark]
        //public void AopService_HasReturnAsync_10000()
        //{
        //    for (int i = 0; i < totalCount; i++)
        //    {
        //        aopService.HasReturnAsync("a");
        //    }
        //    //var aopService = new TestService_Aop2(null);

        //}
        //#endregion
    }


}