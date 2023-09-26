using System.Collections.Generic;
using System.Linq;

namespace Mic.Helpers
{
    public static class ListHelper
    {
        /// <summary>
        /// 把list按照指定数量分隔
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static List<List<T>> SplitList<T>(List<T> list, int length)
        {
            if (list == null || list.Count <= 0 || length <= 0)
            {
                return new List<List<T>>();
            }

            var result = new List<List<T>>();
            var count = list.Count / length;
            count += list.Count % length > 0 ? 1 : 0;
            for (var i = 0; i < count; i++)
            {
                result.Add(list.Skip(i * length).Take(length).ToList());
            }
            return result;
        }
    }
}