using System;
using System.Collections.Generic;

namespace Mic.Aop.Generator.MetaData
{
    /// <summary>
    /// 接口元数据
    /// </summary>
    public class InterfaceMetaData : MetaDataBase, IEquatable<InterfaceMetaData>
    {
        public string Key => $"{Namespace}.{Name}";

        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public InterfaceMetaData(string @namespace, string name, List<AttributeMetaData> attributeMetaData, List<PropertyMetaData> propertyMeta, List<MethodMetaData> methodMetaData, string accessModifier, string extModifier) : base(name, accessModifier, extModifier, attributeMetaData)
        {
            Namespace = @namespace;
            PropertyMeta = propertyMeta;
            MethodMetaData = methodMetaData;
        }

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// 属性集合
        /// </summary>
        public List<PropertyMetaData> PropertyMeta { get; set; }

        /// <summary>
        /// 方法集合
        /// </summary>
        public List<MethodMetaData> MethodMetaData { get; set; }

        public bool Equals(InterfaceMetaData other)
        {
            return Key == other?.Key;
        }

        /// <summary>
        /// 获取哈希
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }
    }
}