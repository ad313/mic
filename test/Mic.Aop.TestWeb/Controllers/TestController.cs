using Mic.Aop.TestWeb.AopSpace;
using Microsoft.AspNetCore.Mvc;
using SaiLing.WaterPlantAttendance.Services.Services.Common;
using System;
using System.Threading.Tasks;

namespace Mic.Aop.TestWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;
        private readonly TestService2 _testService2;
        private readonly TaskSchedulerService _taskSchedulerService;

        public TestController(ITestService testService, TestService2 testService2, TaskSchedulerService taskSchedulerService)
        {
            _testService = testService;
            _testService2 = testService2;
            _taskSchedulerService = taskSchedulerService;
        }

        [HttpGet("GetDateTimeSync")]
        public DateTime GetDateTimeSync()
        {
            var result = _testService.GetDateTimeSync();

            //_testService2.GetDateTimeSync();

            return result;
        }

        [HttpGet("GetDateTimeSyncDirect")]
        public DateTime GetDateTimeSyncDirect()
        {
            var result = _testService.GetDateTimeSyncDirect();
            return result;
        }




        [HttpGet("GetDateTimeAsync")]
        public async Task<DateTime> GetDateTimeAsync()
        {
            var result = await _testService.GetDateTimeAsync();
            return result;
        }

        [HttpGet("GetDateTimeAsyncDirect")]
        public async Task<DateTime> GetDateTimeAsyncDirect()
        {
            var result = await _testService.GetDateTimeAsyncDirect();
            return result;
        }

        [HttpGet("Test")]
        public async Task Test()
        {
            await _taskSchedulerService.InitCalendar();
        }
    }
}
