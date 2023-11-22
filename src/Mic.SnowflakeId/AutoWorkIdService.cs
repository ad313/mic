using FreeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Mic.SnowflakeId
{
    /// <summary>
    /// 雪花ID 自动注册 WorkId
    /// </summary>
    public class AutoWorkIdService : BackgroundService
    {
        private readonly ILogger<AutoWorkIdService> _logger;
        private readonly IdGenConfig _config;
        private RedisClient _redisClient;
        private string _workIdKey;

        /// <summary>
        /// 最大支持节点数
        /// </summary>
        public static int MaxWorkId = 64;
        /// <summary>
        /// 获取到WorkId后回调事件
        /// </summary>
        public static Action<long> RegisterSuccess;

        public AutoWorkIdService(IConfiguration configuration, ILogger<AutoWorkIdService> logger)
        {
            _logger = logger;
            _config = configuration.GetSection("IdGenConfig").Get<IdGenConfig>() ?? new IdGenConfig();
        }

        /// ExecuteAsync
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Start:
            var success = true;
            try
            {
                await BeginRegisterAsync(stoppingToken);
            }
            catch (Exception e)
            {
                success = false;
                _logger.LogWarning($"雪花ID注册服务 注册失败：{e.Message}", e);
            }

            if (!success)
            {
                await Task.Delay(60 * 1000, stoppingToken);
                goto Start;
            }
        }

        /// Stop
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await _redisClient.DelAsync(_workIdKey);
        }

        private async Task BeginRegisterAsync(CancellationToken stoppingToken)
        {
            InfoLog($"启动，检查是否开启：{_config.Enable}");

            if (!_config.Enable)
                return;

            if (string.IsNullOrWhiteSpace(_config.Redis))
                throw new ArgumentNullException(nameof(_config.Redis));

            _redisClient = new RedisClient(_config.Redis);

            //获取index
            var indexKey = FormatKey("Index");
            var name = Assembly.GetEntryAssembly()?.GetName().FullName ?? string.Empty;

            long index;
            long workId;

            do
            {
                index = await _redisClient.IncrAsync(indexKey);
                workId = index % MaxWorkId;

                InfoLog($"获取 Index：{index}，计算 WorkId：{workId}");
                _workIdKey = FormatKey("WorkId", workId.ToString());
            }
            while (await _redisClient.ExistsAsync(_workIdKey));

            InfoLog($"最终 Index：{index}，最终 WorkId：{workId}");

            await _redisClient.SetAsync(_workIdKey, name, _config.HandlerSeconds);

            RegisterSuccess?.Invoke(workId);

            InfoLog($"注册 WorkId {workId} 到 redis，key：{_workIdKey}，有效期：{_config.HandlerSeconds} 秒");

            await UpdateRegisterAsync(_workIdKey, _config.HandlerSeconds, stoppingToken);
        }

        /// 定时更新有效期
        private async Task UpdateRegisterAsync(string key, int second, CancellationToken token)
        {
            try
            {
                var interval = second / 2;
                InfoLog($"启动定时续期，{interval} 秒执行一次");

                var timer = new PeriodicTimer(TimeSpan.FromSeconds(interval));
                while (await timer.WaitForNextTickAsync(token))
                {
                    var item = await _redisClient.GetAsync(key);
                    await _redisClient.SetAsync(key, item, second);

                    _logger.LogDebug($"{DateTime.Now} 雪花ID注册服务 定时续期 {interval} 秒");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{DateTime.Now} 雪花ID注册服务 定时续期失败 {e.Message}", e);
            }

            //重新开始
            InfoLog("定时续期失败 重新注册......");

            await Task.Delay(60 * 1000, token);
            await ExecuteAsync(token);
        }

        private string FormatKey(params string[] key) => $"IdGen:{string.Join(":", key)}";

        private void InfoLog(string message) => _logger.LogInformation($"雪花ID注册服务 {message}");
    }

    public class IdGenConfig
    {
        /// <summary>
        /// Redis 连接字符串
        /// </summary>
        public string Redis { get; set; }

        /// <summary>
        /// 每次占用时间秒数，占用期间不能被其他应用使用
        /// </summary>
        public int HandlerSeconds { get; set; } = 30;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }
    }
}