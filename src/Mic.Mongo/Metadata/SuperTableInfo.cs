using System;
using System.Reflection;
using Mic.Mongo.Models;

namespace Mic.Mongo.Metadata
{
    /// <summary>
    /// 超级表处理
    /// </summary>
    public class SuperTableInfo
    {
        /// <summary>
        /// 表参数
        /// </summary>
        public TableAttribute TableAttribute { get; private set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string Database { get; private set; }

        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// 类型
        /// </summary>
        public Type Type { get; private set; }

        private static HostModel HostModel { get; set; }

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="host"></param>
        public SuperTableInfo(Type type, HostModel host)
        {
            ResolverSuperTable(type, host);
        }

        #endregion

        #region 表解析

        private void ResolverSuperTable(Type type, HostModel host)
        {
            Type = type;
            HostModel = host;
            TableAttribute = type.GetCustomAttribute<TableAttribute>();

            TableName = !string.IsNullOrWhiteSpace(TableAttribute?.Name) ? TableAttribute.Name : type.Name;
            Database = TableAttribute?.Database ?? HostModel.DefaultDatabases;

            if (string.IsNullOrWhiteSpace(Database))
                throw new Exception($"MongoDb {TableName} 未指定数据库");
        }

        #endregion
    }
}