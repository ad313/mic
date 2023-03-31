using System;

namespace SaiLing.WaterPlantAttendance.Services.Common.QuartzTool
{
    /// <summary>
    /// Quartz 定时服务标记
    /// </summary>
    public class QzTagAttribute : Attribute
    {
        /// <summary>
        /// Cron 表达式，优先级最高
        /// </summary>
        public string Cron { get; set; }

        /// <summary>
        /// 指定秒数循环一次
        /// </summary>
        public int Second { get; set; }
    }
}