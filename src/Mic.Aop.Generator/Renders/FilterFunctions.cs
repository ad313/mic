using Mic.Aop.Generator.Extend;
using Mic.Aop.Generator.MetaData;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mic.Aop.Generator.Renders
{
    // We simply inherit from ScriptObject
    // All functions defined in the object will be imported
    public class FilterFunctions : ScriptObject
    {
        /// <summary>
        /// 通过空格分割字符串，返回特定 index 的数据
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string SplitStringByWhiteSpace(string text, int index)
        {
            var arr = Regex.Split(text, "\\s+");
            if (arr.Length >= index)
                return arr[index];
            return null;
        }

        /// <summary>
        /// 判断 class 是否有指定的特性
        /// </summary>
        /// <param name="data"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static bool ClassHasAttribute(ClassMetaData data, string attributeName)
        {
            return data.AttributeMetaData.HasAttribute(attributeName);
        }

        /// <summary>
        /// 获取特性 指定 key 的值
        /// </summary>
        /// <param name="attributeMetaDatas"></param>
        /// <param name="attributeName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetAttributeParamValueByAttributeList(List<AttributeMetaData> attributeMetaDatas, string attributeName, string key)
        {
            var attr = attributeMetaDatas.FirstOrDefault(d => d.Name == attributeName || d.Name + "Attribute" == attributeName);
            if (attr == null)
                return null;

            if (attr.ParamDictionary.TryGetValue(key, out string v))
            {
                return v;
            }

            return null;
        }

        #region Property

        /// <summary>
        /// 判断属性是否有指定的特性
        /// </summary>
        /// <param name="data"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static bool PropertyHasAttribute(PropertyMetaData data, string attributeName)
        {
            return data.AttributeMetaData.HasAttribute(attributeName);
        }

        /// <summary>
        /// 属性列表过滤 特性名称
        /// </summary>
        /// <param name="data"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static List<PropertyMetaData> PropertyListAttributeFilter(List<PropertyMetaData> data, string attributeName)
        {
            return data.Where(d => d.AttributeMetaData.HasAttribute(attributeName)).ToList();
        }

        /// <summary>
        /// 属性列表过滤 特性名称、特性 Key 和 值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="attributeName"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<PropertyMetaData> PropertyListAttributeWithParamFilter(List<PropertyMetaData> data, string attributeName, string key, string value)
        {
            return data.Where(d => d.AttributeMetaData.Any(t => (t.Name == attributeName || t.Name + "Attribute" == attributeName) && t.ParamDictionary.Any(dic => dic.Key == key && dic.Value == value))).ToList();
        }

        /// <summary>
        /// 获取特性 指定 key 的值
        /// </summary>
        /// <param name="propertyMetaData"></param>
        /// <param name="attributeName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetAttributeParamValue(PropertyMetaData propertyMetaData, string attributeName, string key)
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

        #endregion

        #region Method

        /// <summary>
        /// 判断方法是否有指定的特性
        /// </summary>
        /// <param name="data"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static bool MethodHasAttribute(MethodMetaData data, string attributeName)
        {
            return data.AttributeMetaData.HasAttribute(attributeName);
        }

        /// <summary>
        /// 方法列表过滤 特性名称
        /// </summary>
        /// <param name="data"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static List<MethodMetaData> MethodListAttributeFilter(List<MethodMetaData> data, string attributeName)
        {
            return data.Where(d => d.AttributeMetaData.HasAttribute(attributeName)).ToList();
        }

        /// <summary>
        /// 方法列表过滤 特性名称、特性 Key 和 值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="attributeName"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<MethodMetaData> MethodListAttributeWithParamFilter(List<MethodMetaData> data, string attributeName, string key, string value)
        {
            return data.Where(d => d.AttributeMetaData.Any(t => (t.Name == attributeName || t.Name + "Attribute" == attributeName) && t.ParamDictionary.Any(dic => dic.Key == key && dic.Value == value))).ToList();
        }
        
        #endregion
        
        /// <summary>
        /// 获取传入的字符串中 第一个不为空的数据
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static string GetFirstNotNullValue(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim('"') : second?.Trim('"');
        }

        public static string Now()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
