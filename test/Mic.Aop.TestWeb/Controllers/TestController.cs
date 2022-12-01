using Mic.Aop.TestWeb.AopSpace;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Mic.Aop.TestWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;

        public TestController(ITestService testService)
        {
            _testService = testService;
        }

        [HttpGet("HasReturnAsync")]
        public async Task<string> HasReturnAsync()
        {
            //_testService.GetNameSync();
            //_testService.SetNameSync("haha", 123);
            var result = await _testService.HasReturnAsync("aop");

            //Console.WriteLine($"---------------------{result}-------------------------------");
            var rng = new Random();
            return result;
        }

        [HttpGet("NoReturnSync")]
        public async Task<string> NoReturnSync()
        {
            _testService.NoReturnSync("haha", 123);

            //Console.WriteLine($"--------------------------------------------------------");
            var rng = new Random();
            return DateTime.Now.ToString();
        }

        [HttpGet("HasReturnSync")]
        public async Task<string> HasReturnSync()
        {
            var result = _testService.HasReturnSync();

            //Console.WriteLine($"-----------------------------{result}---------------------------");
            var rng = new Random();
            return result;
        }
    }
}
