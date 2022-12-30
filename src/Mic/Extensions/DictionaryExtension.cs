using System.Collections.Generic;

namespace Mic.Extensions
{
    /// <summary>
    /// 字典扩展
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        /// 获取字典值
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetValue(this Dictionary<int, string> dic, int key, string defaultValue = "")
        {
            if (dic == null) return defaultValue;
            return !dic.ContainsKey(key) ? defaultValue : dic[key];
        }

        /// <summary>
        /// 获取字典值
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetValue(this Dictionary<string, string> dic, string key, string defaultValue = "")
        {
            if (dic == null) return defaultValue;
            if (!dic.ContainsKey(key)) return defaultValue;
            return dic[key];
        }
    }
}
