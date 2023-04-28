using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Mic.BenchmarkDotNet.PoolCache;

namespace Mic.BenchmarkDotNet
{
    [MemoryDiagnoser]
    public class CreateInstanceBenchmark
    {
        private CreateInstanceHelper helper = new CreateInstanceHelper();

        private List<ItemModel> list1 = new List<ItemModel>();
        private List<ItemModel> list2 = new List<ItemModel>();

        [Benchmark(Baseline = true)]
        public void Get_dir()
        {
            new ItemModel();
        }

        [Benchmark]
        public void Get_Func()
        {
            helper.CreateInstance<ItemModel>();
        }
    }



}