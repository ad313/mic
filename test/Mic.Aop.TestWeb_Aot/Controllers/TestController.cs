using Microsoft.AspNetCore.Mvc;

namespace Mic.Aop.TestWeb_Aot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
       

        public TestController()
        {
            
        }

        [HttpGet("GetDateTimeSync")]
        public DateTime GetDateTimeSync()
        {
            return DateTime.Now;
        }

    }
}
