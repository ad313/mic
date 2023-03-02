using Mic.Aop.Generator.Extend;
using Mic.Aop.Generator.MetaData;
using Scriban.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mic.Aop.Generator.Renders
{
    public class TemplateClassMetaData : ClassMetaData
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public TemplateClassMetaData(string @namespace, string name, List<AttributeMetaData> attributeMetaData, List<PropertyMetaData> propertyMeta, List<MethodMetaData> methodMetaData, List<string> interfaces, List<KeyValueModel> constructor, List<string> usings, string accessModifier) : base(@namespace, name, attributeMetaData, propertyMeta, methodMetaData, interfaces, constructor, usings, accessModifier)
        {
        }

        public bool HasAttribute(string attributeName)
        {
            return AttributeMetaData.HasAttribute(attributeName);
        }
    }

    // We simply inherit from ScriptObject
    // All functions defined in the object will be imported
    public class FilterFunctions : ScriptObject
    {
        // A function an optional argument
        public static string SplitString(string text, int index)
        {
            var arr = Regex.Split(text, "\\s+");
            if (arr.Length >= index)
                return arr[index];
            return null;
        }

        public static bool ClassHasAttribute(TemplateClassMetaData data, string attributeName)
        {
            return data.HasAttribute(attributeName);
        }

        public static bool PropertyHasAttribute(PropertyMetaData data, string attributeName)
        {
            return data.AttributeMetaData.HasAttribute(attributeName);
        }

        public static List<PropertyMetaData> PropertyListAttributeFilter(List<PropertyMetaData> data, string attributeName)
        {
            return data.Where(d => d.AttributeMetaData.HasAttribute(attributeName)).ToList();
        }

        public static List<PropertyMetaData> PropertyListAttributeWithParamFilter(List<PropertyMetaData> data, string attributeName, string key, string value)
        {
            return data.Where(d => d.AttributeMetaData.Any(t => (t.Name == attributeName || t.Name + "Attribute" == attributeName) && t.ParamDictionary.Any(dic => dic.Key == key && dic.Value == value))).ToList();
        }

        public static bool MethodHasAttribute(MethodMetaData data, string attributeName)
        {
            return data.AttributeMetaData.HasAttribute(attributeName);
        }

        public static string GetAttributeParamValue(PropertyMetaData propertyMetaData,
            string attributeName, string key)
        {
            var attr = propertyMetaData.AttributeMetaData.FirstOrDefault(d => d.Name == attributeName || d.Name + "Attribute" == attributeName);
            if (attr == null)
                return null;

            if (attr.ParamDictionary.TryGetValue(key, out string v))
            {
                return v;
            }

            return null;
        }

        public static string GetValue(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim('"') : second?.Trim('"');
        }
    }
}
