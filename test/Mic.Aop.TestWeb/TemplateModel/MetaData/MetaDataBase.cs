using System.Collections.Generic;
using System.Linq;

namespace Mic.Aop.Generator.MetaData
{
    public class MetaDataBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Attribute 参数
        /// </summary>
        public List<AttributeMetaData> AttributeMetaData { get; private set; }

        public MetaDataBase(string name, List<AttributeMetaData> attributeMetaData)
        {
            Name = name;
            AttributeMetaData = attributeMetaData;
        }

        /// <summary>
        /// 方法唯一标识符，区分方法重载
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string GetKey(string name, List<KeyValueModel> param) => $"{name}_{string.Join("_", param.Select(d => $"{d.Key}_{d.Value}"))}";
    }
}