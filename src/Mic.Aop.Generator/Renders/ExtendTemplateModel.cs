using Mic.Aop.Generator.Extend;
using Mic.Aop.Generator.MetaData;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Mic.Aop.Generator.Renders
{
    /// <summary>
    /// 扩展模板数据模型
    /// </summary>
    public class ExtendTemplateModel //: RazorEngineTemplateBase
    {
        public ExtendTemplateType Type { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public List<string> Templates { get; set; }



        public ConcurrentDictionary<string, string> TemplateDictionary { get; set; } = new ConcurrentDictionary<string, string>();

        public ConcurrentDictionary<string, string> TemplateResult { get; set; } = new ConcurrentDictionary<string, string>();

        public AssemblyMetaData AopMetaDataModel { get; set; }

        public ClassMetaData ClassMetaData { get; set; }

        public InterfaceMetaData InterfaceMetaData { get; set; }
        

        /// <summary>
        /// class 过滤
        /// </summary>
        public string class_filter_expression { get; set; }
        

        public string GetName(string template)
        {
            return $"{Code}_{template}";
        }

        public bool HasAttribute(string name)
        {
            return ClassMetaData?.AttributeMetaData.HasAttribute(name) == true;
        }

        public bool HasAttribute(string name, List<AttributeMetaData> attributeMetaDataList)
        {
            return attributeMetaDataList?.HasAttribute(name) == true;
        }

        public bool PropertyHasAttribute(string name)
        {
            return ClassMetaData.PropertyMeta.Any(d => d.AttributeMetaData.HasAttribute(name));
        }



        public LinqHelper LinqHelper = new LinqHelper();

    }

    /// <summary>
    /// 扩展模板的类型
    /// </summary>
    public enum ExtendTemplateType
    {
        /// <summary>
        /// 类
        /// </summary>
        ClassTarget = 1,
        /// <summary>
        /// 接口
        /// </summary>
        InterfaceTarget = 2,
    }

    public class LinqHelper
    {
        public dynamic First(IEnumerable<dynamic> source) => source.First();
    }
}
