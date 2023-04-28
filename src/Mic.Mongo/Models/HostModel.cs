using System;
using MongoDB.Driver;

namespace Mic.Mongo.Models
{
    public class HostModel
    {
        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 使用Ssl
        /// </summary>
        public bool UseTls { get; set; }

        /// <summary>
        /// 默认数据库
        /// </summary>
        public string DefaultDatabases { get; set; }

        /// <summary>
        /// 获取链接
        /// </summary>
        /// <returns></returns>
        public MongoClientSettings GetMongoClientSettings()
        {
            Check();
            return new MongoClientSettings
            {
                Server = new MongoServerAddress(Host, Port),
                UseTls = UseTls
            };
        }

        private void Check()
        {
            if (string.IsNullOrWhiteSpace(Host))
                throw new Exception("MongoDb Host 不能为空");

            if (Port <= 0)
                throw new Exception("MongoDb Port 不正确");
        }
    }
}