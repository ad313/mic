using Mic.Helpers;
using Mic.SnowflakeId;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SnowflakeExtension
    {
        /// <summary>
        /// 注册 雪花ID，主要用于单个程序多个实例的情况，需要自动注册 WorkId 防止重复，依赖配置，节点名称：IdGenConfig，没有配置则不启用
        ///   "IdGenConfig": {
        ///                    "Redis": "192.168.0.124:6379,password=123456,defaultDatabase=10",
        ///                    "HandlerSeconds": 100,//有效期 秒
        ///                    "Enable": true
        ///                   }
        /// </summary>
        /// <param name="services"></param>
        /// <param name="workerIdBit">雪花ID节点相关，默认5</param>
        /// <returns></returns>
        public static IServiceCollection RegisterSnowflakeId(this IServiceCollection services, int workerIdBit = 5)
        {
            AutoWorkIdService.MaxWorkId = (int)Math.Pow(2, IdHelper.GetWorkerIdBits());

            AutoWorkIdService.RegisterSuccess = workId =>
            {
                IdHelper.SetWorkerIdBits(workerIdBit);
                IdHelper.SetWorkId(workId);

                Console.WriteLine($"雪花ID - WorkId:{workId}");
            };

            services.AddHostedService<AutoWorkIdService>();

            return services;
        }
    }
}