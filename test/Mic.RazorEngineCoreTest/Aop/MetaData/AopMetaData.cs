using System.Collections.Generic;
using System.Linq;

namespace Mic.Aop.Generator.MetaData
{
    /// <summary>
    /// Aop 扫描接口、类元数据
    /// </summary>
    public class AopMetaData
    {
        /// <summary>
        /// 忽略Aop
        /// </summary>
        public string IgnoreAttribute { get; private set; }

        /// <summary>
        /// 所有的AopInterceptor
        /// </summary>
        public List<string> AopAttributeList { get; private set; }

        /// <summary>
        /// 所有的AopInterceptor
        /// </summary>
        public List<ClassMetaData> AopAttributeClassMetaDataList = new List<ClassMetaData>();

        /// <summary>
        /// 接口元数据
        /// </summary>
        public List<InterfaceMetaData> InterfaceMetaDataList { get; private set; }
        /// <summary>
        /// 类元数据
        /// </summary>
        public List<ClassMetaData> ClassMetaDataList { get; private set; }

        public AopMetaData(List<string> aopAttributeList, string ignoreAttribute, List<InterfaceMetaData> interfaceMetaDataList, List<ClassMetaData> classMetaDataList)
        {
            AopAttributeList = aopAttributeList;
            IgnoreAttribute = ignoreAttribute;
            InterfaceMetaDataList = interfaceMetaDataList;
            ClassMetaDataList = classMetaDataList;
        }

        //public List<AopCodeBuilderMetaData> GetAopCodeBuilderMetaData()
        //{
        //    //就近原则，方法 > 类 > 接口方法 > 接口

        //    var list = new List<AopCodeBuilderMetaData>();
        //    foreach (var classMetaData in ClassMetaDataList.Where(d => !AopAttributeList.Contains(d.Name)))
        //    {
        //        ////必须要可重写方法 放出错误
        //        //if (classMetaData.MethodMetaData.All(d => !d.CanOverride))
        //        //    continue;

        //        var methods = new List<MethodMetaData>();
        //        var classHasIgnore = classMetaData.HasIgnore(IgnoreAttribute);

        //        //实现的接口
        //        classMetaData.Usings.Add(classMetaData.NameSpace);
        //        classMetaData.InterfaceMetaData = InterfaceMetaDataList.Where(d => classMetaData.Interfaces.Contains(d.Key)
        //            || classMetaData.Interfaces.SelectMany(t => classMetaData.Usings.Select(u => $"{u.Replace("using ", "").Replace(";", "")}.{t.Split('.').Last()}")).Contains(d.Key)).ToList();
        //        classMetaData.Usings.Remove(classMetaData.NameSpace);

        //        //按照就近原则过滤
        //        //foreach (var methodMetaData in classMetaData.MethodMetaData.Where(d => d.CanOverride))
        //        foreach (var methodMetaData in classMetaData.MethodMetaData)
        //        {
        //            //忽略
        //            if (methodMetaData.AttributeMetaData.HasIgnore(IgnoreAttribute))
        //                continue;

        //            //类方法标记
        //            var methodAttrs = methodMetaData.AttributeMetaData.GetAopAttributes(AopAttributeList);
        //            if (methodAttrs.Any())
        //            {
        //                methodMetaData.AttributeMetaData.Clear();
        //                methodMetaData.AttributeMetaData.AddRange(methodAttrs);
        //                methods.Add(methodMetaData);
        //                continue;
        //            }

        //            //类标记
        //            if (classHasIgnore)
        //                continue;

        //            var classAttr = classMetaData.AttributeMetaData.GetAopAttribute(AopAttributeList);
        //            if (classAttr != null)
        //            {
        //                methodMetaData.AttributeMetaData.Clear();
        //                methodMetaData.AttributeMetaData.Add(classAttr);
        //                methods.Add(methodMetaData);
        //                continue;
        //            }

        //            //接口标记
        //            if (!classMetaData.Interfaces.Any())
        //                continue;

        //            //接口方法忽略
        //            if (classMetaData.InterfaceMetaData.Any(d => d.MethodMetaData.FirstOrDefault(m => m.Key == methodMetaData.Key)?.AttributeMetaData.HasIgnore(IgnoreAttribute) == true))
        //                continue;

        //            //接口方法标记
        //            var interfaceMethodAttr = classMetaData.InterfaceMetaData.Select(d => d.MethodMetaData.FirstOrDefault(m => m.Key == methodMetaData.Key)?.AttributeMetaData.GetAopAttribute(AopAttributeList))
        //                .FirstOrDefault(d => d != null);

        //            if (interfaceMethodAttr != null)
        //            {
        //                methodMetaData.AttributeMetaData.Clear();
        //                methodMetaData.AttributeMetaData.Add(interfaceMethodAttr);
        //                methods.Add(methodMetaData);
        //                continue;
        //            }

        //            //接口标记
        //            var interfaceAttr = classMetaData.InterfaceMetaData.Where(d => d.MethodMetaData.Any(d => d.Key == methodMetaData.Key)).Select(d => d.AttributeMetaData.GetAopAttribute(AopAttributeList))
        //                .FirstOrDefault(d => d != null);
        //            if (interfaceAttr != null)
        //            {
        //                methodMetaData.AttributeMetaData.Clear();
        //                methodMetaData.AttributeMetaData.Add(interfaceAttr);
        //                methods.Add(methodMetaData);
        //                continue;
        //            }
        //        }

        //        if (methods.Any())
        //            list.Add(new AopCodeBuilderMetaData(classMetaData.NameSpace, classMetaData.Name, methods, classMetaData.Constructor, classMetaData.Usings, classMetaData.InterfaceMetaData));
        //    }

        //    return list;
        //}
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
        /// 应用
        /// </summary>
        public List<string> Usings { get; set; }

        /// <summary>
        /// 继承的接口
        /// </summary>
        public List<InterfaceMetaData> InterfaceMetaData { get; set; }

        public AopCodeBuilderMetaData(string nameSpace, string name, List<MethodMetaData> methodMetaDatas, List<KeyValueModel> constructor, List<string> usings, List<InterfaceMetaData> interfaceMetaData)
        {
            NameSpace = nameSpace;
            Name = name;
            MethodMetaDatas = methodMetaDatas;
            Constructor = constructor;
            Usings = usings;
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