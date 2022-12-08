using BenchmarkDotNet.Attributes;
using Mic.BenchmarkDotNet.AopTest.MicAop;
using Mic.BenchmarkDotNet.AopTest.Pool;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

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
            var serviceProvider = Program.Register();
            directService = serviceProvider.GetRequiredService<TestService>();
            aopService = serviceProvider.GetRequiredService<TestService_Aop>();

            //PollHelper.Init();
        }

        [Benchmark(Baseline = true)]
        public void DirectService_GetDateTimeSyncDirect()
        {
            //var directService = new TestService();
            directService.GetDateTimeSyncDirect();
        }

        [Benchmark]
        public void AopService_GetDateTimeSync()
        {
            aopService.GetDateTimeSync();
        }


        [Benchmark]
        public void AopService_SampleSync()
        {
            aopService.SampleSync();
        }














        [Benchmark]
        public async Task DirectService_GetDateTimeAsyncDirect()
        {
            //var aopService = new TestService_Aop2(null);
            await directService.GetDateTimeAsyncDirect();
        }

        [Benchmark]
        public async Task AopService_GetDateTimeAsync()
        {
            await aopService.GetDateTimeAsync();
        }


        [Benchmark]
        public async Task AopService_SampleAsync()
        {
           await aopService.SampleAsync();
        }

        //[Benchmark]
        //public void Rougamo_GetDateTimeSync()
        //{
        //    //var directService = new TestService();
        //    directService.GetDateTimeSync();
        //}

        //[Benchmark]
        //public async Task Rougamo_GetDateTimeAsync()
        //{
        //    //var aopService = new TestService_Aop2(null);
        //    await directService.GetDateTimeAsync();
        //}
    }


}