using BenchmarkDotNet.Attributes;
using Mapster;
using Microsoft.Extensions.ObjectPool;
using SaiLing.Modules.Datas.TdEngine.Pool;
using SaiLing.WaterMetering.Redis.PoolCache;
using System;

namespace Mic.BenchmarkDotNet
{
    [MemoryDiagnoser]
    public class DeepCopyBenchmark
    {
        private static TestPolicy Policy { get; set; }
        private static DefaultObjectPool<ItemModel> ConnectionPool { get; set; }

        public DeepCopyBenchmark()
        {
            Policy = new TestPolicy();
            ConnectionPool = new DefaultObjectPool<ItemModel>(Policy, 10);
            for (int i = 0; i < 10; i++)
            {
                var model = ConnectionPool.Get();
                ConnectionPool.Return(model);
            }
        }

        private ItemModel GetModel()
        {
            return new ItemModel()
            {
                Id = 1,
                Name = "aaaaa",
                Money = 1234455m,
                Time = DateTime.Now,
                Enable = true
            };
        }

        private ItemModel GetModel2()
        {
            var model = ConnectionPool.Get();
            model.Id = 1;
            model.Name = "aaaaa";
            model.Money = 1234455m;
            model.Time= DateTime.Now;
            model.Enable = true;

            return model;
        }

        private ItemModel Convert(ItemModel model)
        {
            return new ItemModel()
            {
                Id = model.Id,
                Name = model.Name,
                Money = model.Money,
                Time = model.Time,
                Enable = model.Enable
            };
        }

        [Benchmark(Baseline = true)]
        public void Get_dir()
        {
            for (int i = 0; i < 1; i++)
            {
                Convert(GetModel());
            }
        }

        [Benchmark]
        public void Get_dir_pool()
        {
            for (int i = 0; i < 1; i++)
            {
                var model = GetModel2();
                Convert(model);
                ConnectionPool.Return(model);
            }
        }

        [Benchmark]
        public void Get_Func()
        {
            for (int i = 0; i < 1; i++)
            {
                DeepCopyHelper.Copy(GetModel());
            }
        }

        [Benchmark]
        public void Get_Func_pool()
        {
            for (int i = 0; i < 1; i++)
            {
                var model = GetModel2();
                DeepCopyHelper.Copy(model);
                ConnectionPool.Return(model);
            }
        }

        [Benchmark]
        public void Get_Mapster()
        {
            for (int i = 0; i < 1; i++)
            {
                GetModel().Adapt<ItemModel>();
            }
        }

        [Benchmark]
        public void Get_Mapster_pool()
        {
            for (int i = 0; i < 1; i++)
            {
                var model = GetModel2();
                model.Adapt<ItemModel>();
                ConnectionPool.Return(model);
                
            }
        }
    }


    public class ItemModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Money { get; set; }

        public DateTime Time { get; set; }

        public bool Enable { get; set; }

        public ItemModel Model { get; set; }
    }

    public class ItemModel2
    {
        public string Code { get; set; }
    }
}