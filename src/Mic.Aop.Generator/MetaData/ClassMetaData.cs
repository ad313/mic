using System.Collections.Generic;

namespace Mic.Aop.Generator.MetaData
{
    /// <summary>
    /// 类元数据
    /// </summary>
    public class ClassMetaData : InterfaceMetaData
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public ClassMetaData(
            string @namespace,
            string name,
            List<AttributeMetaData> attributeMetaData,
            List<PropertyMetaData> propertyMeta,
            List<MethodMetaData> methodMetaData,
            List<string> interfaces,
            List<KeyValueModel> constructor,
            List<string> usings,
            string accessModifier,
            string extModifier = null)
            : base(@namespace, name, attributeMetaData, propertyMeta, methodMetaData, accessModifier, extModifier)
        {
            Constructor = constructor;
            Usings = usings;
            Interfaces = interfaces;
        }

        /// <summary>
        /// 继承的接口
        /// </summary>
        public List<string> Interfaces { get; set; }

        /// <summary>
        /// 继承的接口
        /// </summary>
        public List<InterfaceMetaData> InterfaceMetaData { get; set; }

        /// <summary>
        /// 构造函数参数
        /// </summary>
        public List<KeyValueModel> Constructor { get; set; }

        /// <summary>
        /// 应用
        /// </summary>
        public List<string> Usings { get; set; }
    }
}