using System;
using System.Threading.Tasks;
using Mic.Aop.TestWeb.InterceptorExtend;
using SaiLing.WaterPlantAttendance.Services.Common.QuartzTool;

namespace SaiLing.WaterPlantAttendance.Services.Services.Common
{
    /// <summary>
    /// 定时任务服务
    /// </summary>
    //[QzTag]
    public class TaskSchedulerService
    {
        
        public TaskSchedulerService()
        {
            
        }

        /// <summary>
        /// 每年10、11、12 月生成第二年的节假日数据
        /// </summary>
        /// <returns></returns>
        [QzTag(Cron = "0 0 13 10 10,11,12 ?")]
        [Log2(AopTag = true)]
        public virtual async Task InitCalendar()
        {
            //await _calendarService.InitAsync(DateTime.Now.Year + 1);
            Console.WriteLine("-----" + DateTime.Now);
        }

        /// <summary>
        /// 每年10、11、12 月生成第二年的节假日数据
        /// </summary>
        /// <returns></returns>
        [QzTag(Cron = "0 0 13 10 10,11,12 ?")]
        public async Task InitCalendar2()
        {
            //await _calendarService.InitAsync(DateTime.Now.Year + 1);
        }
    }
}
