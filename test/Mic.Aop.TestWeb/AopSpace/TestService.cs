using System;
using System.Threading.Tasks;

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
    
    [IgnoreAop]
    public class TestService : ITestService
    {
        public TestService()
        {
        }

        [Cache]
        public virtual string HasReturnSync()
        {
            //Console.WriteLine($"method: GetNameSync");
            return "ad313";
        }

        [Log]
        public virtual void NoReturnSync(string name, int id)
        {
            //Console.WriteLine($"method:set name:{name}");
        }

        [Cache]
        public virtual Task<string> HasReturnAsync(string name)
        {
            //Console.WriteLine("excuter.....");
            return Task.FromResult($"ad313 async:{name}");
        }

        [Log]
        public virtual async Task<string> HasReturnAsync(string name, int aaa)
        {
            return await Task.FromResult($"ad313_2 async:{name}");
        }

        [Log]
        public virtual Task<string> Test(string name, int a, AopContext aopContext1)
        {
            throw new NotImplementedException();
        }
    }
}
