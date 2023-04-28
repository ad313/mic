using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Order;
using Perfolizer.Horology;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;
using Mic.Aop.TestWeb.AopSpace;
using Mic.BenchmarkDotNet.AopTest;
using Microsoft.Extensions.DependencyInjection;
using Mic.BenchmarkDotNet.AopTest.MicAop;
using Mic.BenchmarkDotNet.PoolCache;
using SaiLing.WaterMetering.Redis.PoolCache;

#if NETCOREAPP3_0_OR_GREATER
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace Mic.BenchmarkDotNet
{
    internal class Program
    {
        //public static void Main(string[] args) =>
        //    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, DefaultConfig.Instance
        //        //.WithSummaryStyle(new SummaryStyle(CultureInfo.InvariantCulture, printUnitsInHeader: false, SizeUnit.B, TimeUnit.Microsecond))

        //    );

        public static IServiceProvider ServiceProvider { get; set; }

        public static void Main(string[] args)
        {
            //var serviceProvider = Register();
            //var ser = serviceProvider.GetService<TestService>();
            //ser.GetDateTimeSync();

            //var summary = BenchmarkRunner.Run<AopBenchmark>();

            //Console.ReadLine();


            var model = new ItemModel()
            {
                Id = 1,
                Name = "aaaaa",
                Money = 1234455m,
                Time = DateTime.Now,
                Enable = true
            };

            model.Model = new ItemModel()
            {
                Id = 1,
                Name = "aaaaa",
                Money = 1234455m,
                Time = DateTime.Now,
                Enable = true
            };

            var ss = DeepCopyHelper.Copy(model);
            ss = DeepCopyHelper.Copy(model);
            ss = DeepCopyHelper.Copy(model);

            var model2 = new ItemModel2() { Code = "123" };
            var model2_copy = DeepCopyHelper.Copy(model2);
            model2_copy = DeepCopyHelper.Copy(model2);
            model2_copy = DeepCopyHelper.Copy(model2);


            //var s = new CreateInstanceHelper().CreateInstance<ItemModel>();


            //var summary = BenchmarkRunner.Run<DictionaryBenchmark>();
            var summary = BenchmarkRunner.Run<DeepCopyBenchmark>();
            //var summary = BenchmarkRunner.Run<CreateInstanceBenchmark>();

            Console.ReadLine();
        }



        // BENCHMARKS GO HERE
        //dotnet run -c Release -f net5.0 -filter "**" --runtimes net5.0 net6.0
        //dotnet run -c Release -f net6.0 -filter "**" --runtimes net6.0


        //ab -n100000 -c80 http://192.168.0.124:5020/WeatherForecast/HasReturnAsync
        //ab -n100000 -c80 http://192.168.0.124:5020/WeatherForecast/HasReturnSync
        //ab -n100000 -c80 http://192.168.0.124:5020/WeatherForecast/NoReturnSync


        public static IServiceProvider Register()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<TestService>();
            services.AddSingleton<TestService_Aop>();

            services.AddTransient<CacheAttribute>();
            services.AddTransient<SampleAttribute>();
            services.AddMemoryCache();


            return services.BuildServiceProvider();
        }

    }

    
}
