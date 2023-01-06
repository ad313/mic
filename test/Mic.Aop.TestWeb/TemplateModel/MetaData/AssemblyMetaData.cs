using System.Collections.Generic;

namespace Mic.Aop.Generator.MetaData
{
    /// <summary>
    /// 扫描接口、类元数据
    /// </summary>
    public class AssemblyMetaData
    {
        /// <summary>
        /// 忽略Aop
        /// </summary>
        public string IgnoreAttribute { get; private set; }

        /// <summary>
        /// AopInterceptor
        /// </summary>
        public List<string> AopAttributeList { get; private set; }

        /// <summary>
        /// AopInterceptor 元数据
        /// </summary>
        public List<ClassMetaData> AopAttributeMetaDataList = new List<ClassMetaData>();

        /// <summary>
        /// Interface 元数据
        /// </summary>
        public List<InterfaceMetaData> InterfaceMetaDataList { get; private set; }
        /// <summary>
        /// 类元数据
        /// </summary>
        public List<ClassMetaData> ClassMetaDataList { get; private set; }

        public AssemblyMetaData(List<string> aopAttributeList, string ignoreAttribute, List<InterfaceMetaData> interfaceMetaDataList, List<ClassMetaData> classMetaDataList)
        {
            AopAttributeList = aopAttributeList;
            IgnoreAttribute = ignoreAttribute;
            InterfaceMetaDataList = interfaceMetaDataList;
            ClassMetaDataList = classMetaDataList;
        }
    }

    /// <summary>
    /// Aop 代码生成元数据
    /// </summary>
    public class AopCodeBuilderMetaData
    {
        /// <summary>
        /// 命名空间
        /// </summary>
        public string NameSpace { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 方法集合
        /// </summary>
        public List<MethodMetaData> MethodMetaDatas { get; set; }

        /// <summary>
        /// 构造函数参数
        /// </summary>
        public List<KeyValueModel> Constructor { get; set; }

        /// <summary>
        /// 引用
        /// </summary>
        public List<string> Using { get; set; }

        /// <summary>
        /// 继承的接口
        /// </summary>
        public List<InterfaceMetaData> InterfaceMetaData { get; set; }

        public AopCodeBuilderMetaData(string nameSpace, string name, List<MethodMetaData> methodMetaDatas, List<KeyValueModel> constructor, List<string> @using, List<InterfaceMetaData> interfaceMetaData)
        {
            NameSpace = nameSpace;
            Name = name;
            MethodMetaDatas = methodMetaDatas;
            Constructor = constructor;
            Using = @using;
            InterfaceMetaData = interfaceMetaData;
        }
    }

    public class KeyValueModel
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public KeyValueModel(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}