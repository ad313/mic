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
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<LogAttribute>();
            services.AddTransient<Log2Attribute>();
            services.AddTransient<Log3Attribute>();

            services.AddSingleton<TestService>();
            services.AddSingleton<TestService_Aop>();
            ServiceProvider = services.BuildServiceProvider();

            //var service = ServiceProvider.GetRequiredService<TestService_Aop>();
            //var result = service.HasReturnSync();


            //PollHelper.Init();
            //var ser = new TestService_Aop2(null);
            //ser.HasReturnSync();
            //ser.HasReturnSync();
            //ser.HasReturnSync();
            //ser.HasReturnSync();
            //ser.HasReturnSync();
            //ser.HasReturnSync();
            //ser.HasReturnSync();
            //ser.HasReturnSync();

            //var ss = new AopBenchmark();
            //ss.AopService_HasReturnSync();
            //ss.DirectService_HasReturnSync();

            var summary = BenchmarkRunner.Run<AopBenchmark>();
            
            Console.ReadLine();
        }



        // BENCHMARKS GO HERE
        //dotnet run -c Release -f net5.0 -filter "**" --runtimes net5.0 net6.0
        //dotnet run -c Release -f net6.0 -filter "**" --runtimes net6.0


        //ab -n100000 -c80 http://192.168.0.124:5020/WeatherForecast/HasReturnAsync
        //ab -n100000 -c80 http://192.168.0.124:5020/WeatherForecast/HasReturnSync
        //ab -n100000 -c80 http://192.168.0.124:5020/WeatherForecast/NoReturnSync

    }
}
