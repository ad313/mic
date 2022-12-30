using System.Collections.Generic;

namespace Mic.Aop.Generator.MetaData
{
    public class PropertyMetaData: MetaDataBase
    {
        public PropertyMetaData(string name, List<AttributeMetaData> attributeMetaData) : base(name, attributeMetaData)
        {
            //Description = AttributeMetaData.GetStringParam("Display", "Name");

            //Code = AttributeMetaData.GetStringParam("BizDictionary", "Code");
            //IsMultiple = AttributeMetaData.GetBoolParam("BizDictionary", "IsMultiple") ?? false;

            //var bizType = AttributeMetaData.GetStringParam("BizDictionary", "BizType");
            //BizType = !string.IsNullOrWhiteSpace(bizType) && System.Enum.GetValues(typeof(BizTypeEnum)).Cast<BizTypeEnum>()
            //    .ToDictionary(d => d.ToString(), d => d).TryGetValue(bizType, out BizTypeEnum value) ? value : BizTypeEnum.Dictionary;
        }

        /// <summary>
        /// 属性描述
        /// </summary>
        public string Description { get; private set; }
    }
}
