using System.Collections.Concurrent;
using System.Collections.Generic;

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

        //public AssemblyMetaData AopMetaDataModel { get; set; }

        //public ClassMetaData ClassMetaData { get; set; }

        //public InterfaceMetaData InterfaceMetaData { get; set; }
        
        /// <summary>
        /// class attribute 过滤
        /// </summary>
        public string class_attribute_filter_expression { get; set; }

        ///// <summary>
        ///// class property attribute 过滤
        ///// </summary>
        //public string class_property_attribute_filter_expression { get; set; }
        

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
}
