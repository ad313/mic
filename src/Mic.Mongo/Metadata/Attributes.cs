using System;
using System.ComponentModel;

namespace Mic.Mongo.Metadata
{
    /// <summary>
    /// Table标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public TableAttribute() { }

        /// <summary>
        /// 表名称
        /// </summary>
        [DisplayName("表名称")]
        public string Name { get; set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        [DisplayName("数据库名称")]
        public string Database { get; set; }
    }
}