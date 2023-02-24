//using System;

//namespace Mic.Extensions
//{
//    public static class Extension
//    {
//        /// <summary>
//        /// 反射 根据名称获取值
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="model"></param>
//        /// <param name="name"></param>
//        /// <param name="type"></param>
//        /// <returns></returns>
//        public static object GetValue<T>(this T model, string name, Type type = null)
//        {
//            if (type == null)
//            {
//                type = typeof(T);
//            }
//            return type?.GetProperty(name)?.GetValue(model, null);
//        }

//        public static string ThrowIfNull(this string str, string name)
//        {
//            if (string.IsNullOrWhiteSpace(str))
//                throw new ArgumentNullException(name);

//            return str;
//        }

//        public static object ThrowIfNull(this object obj, string name)
//        {
//            if (obj == null)
//                throw new ArgumentNullException(name);

//            return obj;
//        }
//    }
//}