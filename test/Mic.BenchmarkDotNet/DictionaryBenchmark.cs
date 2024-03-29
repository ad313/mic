﻿using BenchmarkDotNet.Attributes;
using Collections.Pooled;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mic.BenchmarkDotNet
{
    [MemoryDiagnoser]
    public class DictionaryBenchmark
    {
        private Dictionary<int, int> dic = new Dictionary<int, int>();
        private ConcurrentDictionary<int, int> concurrentDictionary = new ConcurrentDictionary<int, int>();
        private PooledDictionary<int, int> pooledDictionary = new PooledDictionary<int, int>();

        public DictionaryBenchmark()
        {
            for (int i = 0; i < 200; i++)
            {
                dic.Add(i,i);
                concurrentDictionary.TryAdd(i, i);
                pooledDictionary.Add(i, i);
            }

            //PollHelper.Init();
        }

        [Benchmark(Baseline = true)]
        public void Get_Dictionary()
        {
            dic.TryGetValue(55, out int v);
        }

        [Benchmark]
        public void Get_ConcurrentDictionary()
        {
            concurrentDictionary.TryGetValue(55, out int v);
        }

        [Benchmark]
        public void Get_PooledDictionary()
        {
            pooledDictionary.TryGetValue(55, out int v);
        }
    }


}