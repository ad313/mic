using Mic.EventBus.RabbitMQ.Attributes;
using Mic.EventBus.RabbitMQ.Core.Abstractions;

namespace Mic.EventBus.Test
{
    public interface IService1
    {
        Task<string> Test1(int index);
        Task<string> Test2(int index);
    }

    public class Service1 : IService1
    {
        private readonly IHttpContextAccessor _accessor;

        public Service1(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        [Subscriber("test1", "test", false)]
        public async Task<string> Test1(int index)
        {
            var ss = _accessor.HttpContext;

            var result = $"Test1 with false {index}";
            Console.WriteLine(result);
            return await Task.FromResult(result);
        }

        [Subscriber("test2", "test", true)]
        public async Task<string> Test12(int index)
        {
            var result = $"Test12 with true {index}";
            Console.WriteLine(result);
            return await Task.FromResult(result);
        }

        [Subscriber("test2", "test", true)]
        public async Task<string> Test2(int index)
        {
            var result = $"Test2 with true {index}";
            Console.WriteLine(result);
            return await Task.FromResult(result);
        }

        [Subscriber("test2", "test", true)]
        public async Task<string> Test3(int index)
        {
            var result = $"Test3 with true {index}";
            Console.WriteLine(result);
            return await Task.FromResult(result);
        }
        

        [RpcServer("Service1/TestRpcServer")]
        public async Task<string> TestRpcServer(int index)
        {
            var ss = _accessor.HttpContext;

            var result = $"TestRpcServer with input {index}";
            Console.WriteLine(result);
            return await Task.FromResult(result);
        }

        [RpcServer("Service1/TestRpcServer2")]
        public async Task<string> TestRpcServer2(int a, string b, decimal c, DateTime d, EventMessageModel<int> e)
        {
            var ss = _accessor.HttpContext;

            var result = $"TestRpcServer with input empty";
            Console.WriteLine(result);
            return await Task.FromResult(result);
        }
    }
}