using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Mic.Extensions
{
    /// <summary>
    /// 枚举扩展类
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取枚举的描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            return value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description;
        }

        /// <summary>
        /// 枚举值和枚举描述转换成字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<int, string> GetItems<T>()
        {
            var result = new Dictionary<int, string>();
            foreach (Enum itemValue in Enum.GetValues(typeof(T)))
            {
                result.Add(Convert.ToInt32(itemValue), itemValue.GetDescription());
            }
            return result;
        }

        /// <summary>
        /// 枚举值和枚举key转换成字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<int, string> GetItemsValue<T>()
        {
            var result = new Dictionary<int, string>();
            foreach (Enum itemValue in Enum.GetValues(typeof(T)))
            {
                result.Add(Convert.ToInt32(itemValue), itemValue.ToString());
            }
            return result;
        }

        /// <summary>
        /// 获取枚举描述，根据 值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string FormatEnumDesText<T>(this int value, string defaultValue = "")
        {
            try
            {
                var dic = GetItems<T>();
                return dic.GetValue(value);
            }
            catch
            {
                return "未知";
            }
        }

        /// <summary>
        /// 获取枚举描述，根据 值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="spi"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string FormatEnumDesText<T>(this List<string> values, char spi = ',', string defaultValue = "")
        {
            try
            {
                if (values == null || values.Count <= 0) return defaultValue;
                var dic = GetItems<T>();

                var str = "";
                foreach (var value in values)
                {
                    if (int.TryParse(value, out int key))
                    {
                        var tempValue = dic.GetValue(key, defaultValue);
                        if (tempValue != defaultValue)
                        {
                            str += tempValue + spi;
                        }
                    }
                }
                return str.TrimEnd(spi);
            }
            catch
            {
                return "未知";
            }
        }

        /// <summary>
        /// 获取枚举key，根据 值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string FormatEnumText<T>(this int value, string defaultValue = "")
        {
            try
            {
                var dic = GetItemsValue<T>();
                return dic.GetValue(value);
            }
            catch
            {
                return "未知";
            }
        }

        /// <summary>
        /// 获取枚举key，根据 值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string FormatEnumTexts<T>(this List<string> values, string defaultValue = "")
        {
            try
            {
                if (values == null || values.Count <= 0) return defaultValue;
                var dic = GetItemsValue<T>();

                var str = "";
                foreach (var value in values)
                {
                    if (int.TryParse(value, out int key))
                    {
                        var tempValue = dic.GetValue(key, defaultValue);
                        if (tempValue != defaultValue)
                        {
                            str += tempValue;
                        }
                    }
                }
                return str;
            }
            catch
            {
                return "未知";
            }
        }
    }
}
