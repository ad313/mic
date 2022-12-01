using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mic.BenchmarkDotNet.AopTest.Pool;
using Microsoft.Extensions.DependencyInjection;
using System.Xml.Linq;

namespace Mic.Aop.TestWeb.AopSpace
{
    //[Log(Index = 1)]
    public interface ITestService
    {
        string HasReturnSync();
        void NoReturnSync(string name, int id);

        Task<string> HasReturnAsync(string name);
        
        Task<string> HasReturnAsync(string name, int aaa);

        Task<string> Test(string name, int a, AopContext aopContext);
    }

    public interface ITestService2
    {
        string HasReturnSync();
        void NoReturnSync(string name, int id);

        Task<string> HasReturnAsync(string name);

        Task<string> HasReturnAsync(string name, int aaa);

        Task<string> Test(string name, int a, AopContext aopContext1);
    }

    [IgnoreAop]
    public class TestService : Mic.Aop.TestWeb.AopSpace.ITestService
    {
        public TestService()
        {
        }

        [Log(Index = 2)]
        //[Log2(Pa = EnumParam.Two,Index = 33)]
        //[Log3]
        public virtual string HasReturnSync()
        {
            //Console.WriteLine($"method: GetNameSync");
            return "ad313";
        }

        [Log(Index = 2)]
        //[Log2(Pa = EnumParam.Three)]
        //[Log3]
        public virtual void NoReturnSync(string name, int id)
        {
            //Console.WriteLine($"method:set name:{name}");
        }

        [Log(Index = 2)]
        //[Log2(Pa = EnumParam.Two)]
        //[Log3]
        public virtual Task<string> HasReturnAsync(string name)
        {
            //Console.WriteLine("excuter.....");
            return Task.FromResult($"ad313 async:{name}");
        }

        [Log(Index = 4)]
        public virtual async Task<string> HasReturnAsync(string name, int aaa)
        {
            return await Task.FromResult($"ad313_2 async:{name}");
        }

        [Log(Index = 4)]
        public virtual Task<string> Test(string name, int a, AopContext aopContext1)
        {
            throw new NotImplementedException();
        }
    }


    
    public class TestService2 //: ITestService2
    {
        public TestService2()
        {
        }

        [Log(Index = 2)]
        [Log2(Pa = EnumParam.Two)]
        [Log3]
        public virtual string HasReturnSync()
        {
            //Console.WriteLine($"method: GetNameSync");
            return "ad313";
        }

        [Log(Index = 2)]
        [Log2(Pa = EnumParam.Three)]
        [Log3]
        public virtual void NoReturnSync(string name, int id)
        {
            //Console.WriteLine($"method:set name:{name}");
        }

        [Log(Index = 2)]
        [Log2(Pa = EnumParam.Two)]
        [Log3]
        public virtual Task<string> HasReturnAsync(string name)
        {
            //Console.WriteLine("excuter.....");
            return Task.FromResult($"ad313 async:{name}");
        }

        [Log(Index = 4)]
        public virtual async Task<string> HasReturnAsync(string name, int aaa)
        {
            return await Task.FromResult($"ad313_2 async:{name}");
        }

        [Log(Index = 4)]
        public virtual Task<string> Test(string name, int a, AopContext aopContext1)
        {
            throw new NotImplementedException();
        }
    }




    public sealed class TestService_Aop : TestService
    {
        private readonly IServiceProvider _serviceProvider0;
        public TestService_Aop(IServiceProvider serviceProvider0)
        {
            _serviceProvider0 = serviceProvider0;
        }

        public override string HasReturnSync()
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { },
                false,
                true,
                null);

            var aopInterceptor0 = _serviceProvider0.GetRequiredService<LogAttribute>();
            aopInterceptor0.Index = 2;

            if (aopInterceptor0.HasBefore)
                aopContext = aopInterceptor0.Before(aopContext);
            
            if (aopInterceptor0.HasAopNext)
            {
                if (aopInterceptor0.HasActualNext)
                {
                    aopContext.ActualMethod = () => base.HasReturnSync();
                    aopContext = aopInterceptor0.Next(aopContext);
                }
                else
                {
                    aopContext = aopInterceptor0.Next(aopContext);
                }
            }
            else
            {
                if (aopInterceptor0.HasActualNext)
                {
                    aopContext.ReturnValue = base.HasReturnSync();
                }
            }

            if (aopInterceptor0.HasAfter)
                aopContext = aopInterceptor0.After(aopContext);

            return aopContext.ReturnValue;
        }

        public string HasReturnSync2()
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { },
                false,
                true,
                null);

            var aopInterceptor0 = _serviceProvider0.GetRequiredService<LogAttribute>();
            aopInterceptor0.Index = 2;

            aopContext = aopInterceptor0.Before(aopContext);


            aopContext.ActualMethod = () => base.HasReturnSync();
            aopContext = aopInterceptor0.Next(aopContext);

            aopContext = aopInterceptor0.After(aopContext);

            return aopContext.ReturnValue;
        }

        public string HasReturnSync3()
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { },
                false,
                true,
                null);

            var aopInterceptor0 = new LogAttribute();
            aopInterceptor0.Index = 2;

            aopContext = aopInterceptor0.Before(aopContext);


            aopContext.ActualMethod = () => base.HasReturnSync();
            aopContext = aopInterceptor0.Next(aopContext);

            aopContext = aopInterceptor0.After(aopContext);

            return aopContext.ReturnValue;
        }

        public string HasReturnSyncNoNext()
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { },
                false,
                true,
                null);

            var aopInterceptor0 = new LogAttribute();
            aopInterceptor0.Index = 2;

            aopContext = aopInterceptor0.Before(aopContext);


            aopContext.ActualMethod = () => base.HasReturnSync();
            aopContext.Invoke();

            aopContext = aopInterceptor0.After(aopContext);

            return aopContext.ReturnValue;
        }

        public string HasReturnSyncNoNextAndAfter()
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { },
                false,
                true,
                null);

            var aopInterceptor0 = new LogAttribute();
            aopInterceptor0.Index = 2;

            aopContext = aopInterceptor0.Before(aopContext);


            aopContext.ActualMethod = () => base.HasReturnSync();
            aopContext.Invoke();

            //aopContext = aopInterceptor0.After(aopContext);

            return aopContext.ReturnValue;
        }

        public string HasReturnSyncNoNextAndAfterBase()
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { },
                false,
                true,
                null);

            var aopInterceptor0 = new LogAttribute();
            aopInterceptor0.Index = 2;

            aopContext = aopInterceptor0.Before(aopContext);

            
            aopContext.ReturnValue = base.HasReturnSync();

            //aopContext = aopInterceptor0.After(aopContext);

            return aopContext.ReturnValue;
        }

        public string HasReturnSyncNoNextAndAfterBasePool()
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { },
                false,
                true,
                null);

            var aopInterceptor0 = PollHelper.LogAttributePolicyPool.Get();
            aopInterceptor0.Index = 2;

            aopContext = aopInterceptor0.Before(aopContext);


            aopContext.ReturnValue = base.HasReturnSync();

            //aopContext = aopInterceptor0.After(aopContext);
            PollHelper.LogAttributePolicyPool.Return(aopInterceptor0);
            return aopContext.ReturnValue;
        }

        public string HasReturnSyncNoNextAndAfterBasePoolServiceProvider()
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { },
                false,
                true,
                null);

            var aopInterceptor0 = _serviceProvider0.GetRequiredService<LogAttribute>();
            aopInterceptor0.Index = 2;

            aopContext = aopInterceptor0.Before(aopContext);


            aopContext.ReturnValue = base.HasReturnSync();

            //aopContext = aopInterceptor0.After(aopContext);

            return aopContext.ReturnValue;
        }

        public override void NoReturnSync(string name, int id)
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { { "name", name }, { "id", id } },
                false,
                false,
                null);

            var aopInterceptor0 = _serviceProvider0.GetRequiredService<LogAttribute>();
            aopInterceptor0.Index = 2;

            aopContext = aopInterceptor0.Before(aopContext);

            aopContext.ActualMethod = () => Task.Run(() => base.NoReturnSync(name, id));
            var result = aopInterceptor0.Next(aopContext);

            result = aopInterceptor0.After(result);
        }

        public override async Task<string> HasReturnAsync(string name)
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { { "name", name } },
                true,
                true,
                null);

            var aopInterceptor0 = _serviceProvider0.GetRequiredService<LogAttribute>();
            aopInterceptor0.Index = 2;

            aopContext = await aopInterceptor0.BeforeAsync(aopContext);

            aopContext.ReturnValue = await base.HasReturnAsync(name);

            //aopContext = await aopInterceptor0.AfterAsync(aopContext);

            return aopContext.ReturnValue;
        }


        public async Task<string> HasReturnAsyncNoNextAndAfterBase(string name)
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { { "name", name } },
                true,
                true,
                null);

            var aopInterceptor0 = new LogAttribute();
            aopInterceptor0.Index = 2;

            aopContext = await aopInterceptor0.BeforeAsync(aopContext);

            aopContext.ReturnValue = await base.HasReturnAsync(name);

            //aopContext = await aopInterceptor0.AfterAsync(aopContext);

            return aopContext.ReturnValue;
        }

        public async Task<string> HasReturnAsyncNoNextAndAfterBasePool(string name)
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { { "name", name } },
                true,
                true,
                null);

            var aopInterceptor0 = PollHelper.LogAttributePolicyPool.Get();
            aopInterceptor0.Index = 2;

            aopContext = await aopInterceptor0.BeforeAsync(aopContext);

            aopContext.ReturnValue = await base.HasReturnAsync(name);

            //aopContext = await aopInterceptor0.AfterAsync(aopContext);
            PollHelper.LogAttributePolicyPool.Return(aopInterceptor0);
            return aopContext.ReturnValue;
        }

        public async Task<string> HasReturnAsyncWithNextAndAfterBasePool(string name)
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { { "name", name } },
                true,
                true,
                null);

            var aopInterceptor0 = _serviceProvider0.GetRequiredService<LogAttribute>();
            aopInterceptor0.Index = 2;

            aopContext = await aopInterceptor0.BeforeAsync(aopContext);

            aopContext.ActualMethod = () => base.HasReturnAsync(name);
            aopContext = await aopInterceptor0.NextAsync(aopContext);

            //aopContext = await aopInterceptor0.AfterAsync(aopContext);
            return aopContext.ReturnValue;
        }







































        public override async Task<string> HasReturnAsync(string name, int aaa)
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { { "name", name }, { "aaa", aaa } },
                true,
                true,
                null);

            var aopInterceptor0 = _serviceProvider0.GetRequiredService<LogAttribute>();
            aopInterceptor0.Index = 4;

            aopContext = await aopInterceptor0.BeforeAsync(aopContext);

            aopContext.ActualMethod = () => base.HasReturnAsync(name, aaa);
            var result = await aopInterceptor0.NextAsync(aopContext);

            result = await aopInterceptor0.AfterAsync(result);

            return result.ReturnValue;
        }

        public override async Task<string> Test(string name, int a, AopContext aopContext1)
        {
            var aopContext = new AopContext(_serviceProvider0,
                new Dictionary<string, dynamic>() { { "name", name }, { "a", a }, { "aopContext1", aopContext1 } },
                true,
                true,
                null);

            var aopInterceptor0 = _serviceProvider0.GetRequiredService<LogAttribute>();
            aopInterceptor0.Index = 4;

            aopContext = await aopInterceptor0.BeforeAsync(aopContext);

            aopContext.ActualMethod = () => base.Test(name, a, aopContext1);
            var result = await aopInterceptor0.NextAsync(aopContext);

            result = await aopInterceptor0.AfterAsync(result);

            return result.ReturnValue;
        }

    }

}
