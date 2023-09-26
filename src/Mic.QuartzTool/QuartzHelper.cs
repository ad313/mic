using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace Mic.QuartzTool
{
    /// <summary>
    /// Quartz 辅助类
    /// </summary>
    public class QuartzHelper
    {
        /// <summary>
        /// 创建一个标准调度器工厂
        /// </summary>
        private static readonly StdSchedulerFactory Factory = new StdSchedulerFactory();

        /// <summary>
        /// 调度器
        /// </summary>
        private static IScheduler _scheduler = null;

        /// <summary>
        /// 运行的job字典
        /// </summary>
        public static Dictionary<string, QuartzConfig> JobDictionary { get; private set; } = new Dictionary<string, QuartzConfig>();

        /// <summary>
        /// 开始执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initConfig">配置文件</param>
        /// <returns></returns>
        public static async Task<QuartzConfig> Init<T>(Action<QuartzConfig> initConfig) where T : IJob
        {
            if (initConfig == null)
                return null;

            var config = new QuartzConfig();
            initConfig.Invoke(config);

            if (string.IsNullOrWhiteSpace(config.GroupName))
                config.GroupName = $"{typeof(T).Name}_Group";
            if (string.IsNullOrWhiteSpace(config.JobName))
                config.JobName = $"{typeof(T).Name}";
            if (string.IsNullOrWhiteSpace(config.TriggerName))
                config.TriggerName = $"{typeof(T).Name}_Trigger";

            if (JobDictionary.ContainsKey(config.JobName))
                throw new Exception($"已存在名称为 {config.JobName} 的Job，创建失败");

            JobDictionary.Add(config.JobName, config);

            //延迟执行
            if (config.FirstDelayLength > 0)
                await Task.Delay(config.FirstDelayLength);

            //通过从标准调度器工厂获得一个调度器，用来启动任务
            if (_scheduler == null)
                _scheduler = await Factory.GetScheduler();

            //调度器的线程开始执行，用以触发Trigger
            await _scheduler.Start();

            //jobDetail
            var jobDetail = CreateJobDetail<T>(config.JobName, config.GroupName);

            if (config.AppendData != null)
            {
                foreach (var keyValuePair in config.AppendData)
                {
                    jobDetail.JobDataMap.Put(keyValuePair.Key, keyValuePair.Value);
                }
            }

            var trigger = string.IsNullOrWhiteSpace(config.Cron)
                ? CreateTrigger(config.JobName, config.GroupName, config.SleepLength)
                : CreateCronTrigger(config.JobName, config.GroupName, config.Cron);

            await _scheduler.ScheduleJob(jobDetail, trigger);

            return config;
        }

        /// <summary>
        /// 关闭Job
        /// </summary>
        /// <param name="jobName"></param>
        /// <returns></returns>
        public static async Task<string> StopAndDeleteJob(string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
                return "jobName 为空";

            if (!JobDictionary.TryGetValue(jobName, out QuartzConfig config))
            {
                return "jobName 不存在";
            }

            await _scheduler.UnscheduleJob(new TriggerKey(config.JobName, config.GroupName));
            await _scheduler.DeleteJob(new JobKey(jobName));
            JobDictionary.Remove(jobName);

            Console.WriteLine($"--{jobName} 已关闭");
            return "关闭成功";
        }

        /// <summary>
        /// 关闭Job
        /// </summary>
        /// <param name="jobNames"></param>
        /// <returns></returns>
        public static async Task<string> StopAndDeleteJobs(params string[] jobNames)
        {
            if (jobNames == null) return "job名不能为空";

            foreach (var jobName in jobNames)
            {
                await StopAndDeleteJob(jobName);
            }

            return "关闭成功";
        }

        /// <summary>
        /// 关闭所有Job
        /// </summary>
        /// <returns></returns>
        public static async Task<string> StopAndDeleteAllJobs()
        {
            if (!JobDictionary.Keys.Any()) return "没有运行中的Job";

            foreach (var jobName in JobDictionary.Keys)
            {
                await StopAndDeleteJob(jobName);
            }

            return "关闭成功";
        }

        /// <summary>
        /// 创建 JobDetail
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        private static IJobDetail CreateJobDetail<T>(string jobName, string groupName) where T : IJob
        {
            return JobBuilder.Create<T>().WithIdentity(jobName, groupName).Build();
        }

        /// <summary>
        /// 创建Trigger
        /// </summary>
        /// <returns></returns>
        private static ITrigger CreateTrigger(string jobName, string groupName, int sleepLength)
        {
            return TriggerBuilder
                .Create()
                .WithIdentity(jobName, groupName)
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(sleepLength).RepeatForever())
                .Build();
        }

        /// <summary>
        /// 创建Trigger
        /// </summary>
        /// <returns></returns>
        private static ITrigger CreateCronTrigger(string jobName, string groupName, string cron)
        {
            return TriggerBuilder
                .Create()
                .WithIdentity(jobName, groupName)
                .StartNow()
                .WithCronSchedule(cron)
                .Build();
        }
    }

    /// <summary>
    /// Quartz 任务配置
    /// </summary>
    public class QuartzConfig
    {
        /// <summary>
        /// Cron表达式
        /// </summary>
        public string Cron { get; set; }

        /// <summary>
        /// 触发间隔 秒
        /// </summary>
        public int SleepLength { get; set; }

        /// <summary>
        /// JobName
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// GroupName
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// TriggerName
        /// </summary>
        public string TriggerName { get; set; }

        /// <summary>
        /// 第一次执行延迟时间 毫秒
        /// </summary>
        public int FirstDelayLength { get; set; }

        /// <summary>
        /// 指定的参数
        /// </summary>
        public Dictionary<string, object> AppendData { get; set; } = new Dictionary<string, object>();
    }
}