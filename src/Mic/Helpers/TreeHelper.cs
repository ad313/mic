//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Mic.Helpers
//{
//    /// <summary>
//    /// 树形结构数据处理
//    /// </summary>
//    public static class TreeHelper
//    {
//        #region 处理树形结构，名称用--分隔，便于区分

//        /// <summary>
//        /// 处理树形结构，名称用--分隔，便于区分
//        /// </summary>
//        /// <param name="source">原始数据集合</param>
//        /// <param name="idName">id字段名称</param>
//        /// <param name="pidName">父id字段名称</param>
//        /// <param name="textName">text字段名称</param>
//        /// <param name="topValue">最顶层父节点的值</param>
//        /// <param name="sortName">排序字段名称</param>
//        /// <param name="sourceSpa">分隔符</param>
//        /// <returns></returns>
//        public static List<T> ForamtTreeDataName<T>(object source, string idName, string pidName, string textName, string topValue, string sortName, string sourceSpa = "--")
//        {
//            var baseArray = JArray.FromObject(source);
//            var list = new JArray();

//            //最上层节点Id集合
//            var topIds = GetTop(source, idName, pidName, topValue, sortName);
//            foreach (var id in topIds)
//            {
//                var top = baseArray.FirstOrDefault(d => d[idName].Value<string>() == id);
//                var child = ForamtTreeDataNameChild(idName, pidName, textName, top, baseArray, "", sourceSpa, sortName);
//                child.ForEach(d => list.Add(d));
//            }

//            var json = JsonConvert.SerializeObject(list, new JsonSerializerSettings()
//            {
//                DateFormatString = "yyyy-MM-dd HH:mm:ss"
//            });
//            return JsonConvert.DeserializeObject<List<T>>(json);
//        }

//        /// <summary>
//        /// 处理树形结构，名称用--分隔，便于区分
//        /// </summary>
//        /// <param name="idName">id字段名称</param>
//        /// <param name="pidName">父id字段名称</param>
//        /// <param name="textName">text字段名称</param>
//        /// <param name="curr">当前项数据</param>
//        /// <param name="originalList">数据集合</param>
//        /// <param name="currSpar">当前分隔</param>
//        /// <param name="sourceSpa">分隔符</param>
//        /// <param name="sortName"></param>
//        /// <returns></returns>
//        private static List<JToken> ForamtTreeDataNameChild(string idName, string pidName, string textName, JToken curr, JArray originalList, string currSpar, string sourceSpa, string sortName)
//        {
//            curr[textName] = currSpar + curr[textName];
//            currSpar = sourceSpa + currSpar;
//            var child = !string.IsNullOrWhiteSpace(sortName)
//                ? originalList.Where(d => d[pidName].Value<string>() == curr[idName].Value<string>()).OrderBy(d => d[sortName]).ToList()
//                : originalList.Where(d => d[pidName].Value<string>() == curr[idName].Value<string>()).ToList();
//            if (child.Count <= 0)
//            {
//                return new List<JToken> { curr };
//            }
//            var childArray = new List<JToken> { curr };
//            foreach (var item in child)
//            {
//                childArray.AddRange(ForamtTreeDataNameChild(idName, pidName, textName, item, originalList, currSpar, sourceSpa, sortName));
//            }
//            return childArray;
//        }

//        #endregion

//        #region 递归，返回本级及所有子级

//        /// <summary>
//        /// 递归，返回本级及所有子级
//        /// </summary>
//        /// <typeparam name="T">数据源类型</typeparam>
//        /// <typeparam name="TId">主键Id类型</typeparam>
//        /// <param name="source">数据总集合</param>
//        /// <param name="currId">当前Id</param>
//        /// <param name="getCurrIdFunc">委托：返回某个实体的主键值</param>
//        /// <param name="getCurrFunc">委托：通过主键值，返回一条数据</param>
//        /// <param name="getChildrenFunc">委托：通过主键值，返回它的下级数据</param>
//        /// <returns></returns>
//        public static IEnumerable<T> GetAllChildren<T, TId>(this IEnumerable<T> source, TId currId, Func<T, TId> getCurrIdFunc, Func<T, TId, bool> getCurrFunc, Func<T, TId, bool> getChildrenFunc)
//            where T : class, new()
//        {
//            if (currId == null) return new List<T>();
//            var result = new List<T>();
//            //获取本级信息
//            var curr = source.FirstOrDefault(d => getCurrFunc(d, currId));
//            if (curr != null)
//            {
//                result.Add(curr);

//                //获取子级
//                var childList = source.Where(d => getChildrenFunc(d, getCurrIdFunc(curr))).ToList();
//                foreach (var item in childList)
//                {
//                    result.AddRange(GetAllChildren(source, getCurrIdFunc(item), getCurrIdFunc, getCurrFunc, getChildrenFunc));
//                }
//            }
//            return result.Distinct().ToList();
//        }

//        #endregion

//        #region 递归，返回所有直属父级

//        /// <summary>
//        /// 递归，返回本级及所有子级
//        /// </summary>
//        /// <typeparam name="T">数据源类型</typeparam>
//        /// <typeparam name="TId">主键Id类型</typeparam>
//        /// <param name="source">数据总集合</param>
//        /// <param name="currId">当前Id</param>
//        /// <param name="getCurrIdFunc">委托：获取当前id</param>
//        /// <param name="getParentIdFunc">委托：获取父级值</param>
//        /// <param name="getCurrFunc">委托：通过主键值，返回一条数据</param>
//        /// <returns></returns>
//        public static IEnumerable<T> GetAllParents<T, TId>(this List<T> source, TId currId, Func<T, TId> getCurrIdFunc, Func<T, TId> getParentIdFunc, Func<T, TId, bool> getCurrFunc)
//            where T : class, new()
//        {
//            if (currId == null) return new List<T>();
//            var result = new List<T>();
//            //获取本级信息
//            var curr = source.FirstOrDefault(d => getCurrFunc(d, currId));
//            if (curr != null)
//            {
//                result.Add(curr);

//                //获取父级
//                var parent = source.FirstOrDefault(d => getCurrFunc(d, getParentIdFunc(curr)));
//                if (parent != null)
//                {
//                    result.AddRange(GetAllParents(source, getCurrIdFunc(parent), getCurrIdFunc, getParentIdFunc, getCurrFunc));
//                }
//            }
//            return result.Distinct().ToList();
//        }

//        #endregion

//        #region 处理树形结构 强类型

//        /// <summary>
//        /// 处理树形结构 TreeDataExtentions.FormatTreeData<TreeClass, int>(list, 0, d => d.ParentId, d => d.Id, (d, id) => d.Id == id, (d, id) => d.ParentId == id, (d, cList) => d.Children = cList);
//        /// </summary>
//        /// <typeparam name="T">数据源类型</typeparam>
//        /// <typeparam name="TId">主键Id类型</typeparam>
//        /// <param name="source">数据总集合</param>
//        /// <param name="topParentValue">最顶层父节点的值</param>
//        /// <param name="getParentIdFunc">委托：返回父节点Id值</param>
//        /// <param name="getCurrIdFunc">委托：返回主键Id值</param>
//        /// <param name="getCurrFunc">委托：通过主键值，返回一条数据</param>
//        /// <param name="getChildrenFunc">委托：通过主键值，返回它的下级数据</param>
//        /// <param name="setChildDataFunc">委托：设置当前节点的下级数据</param>
//        /// <param name="toEnd">递归到最底部时执行</param>
//        /// <param name="level"></param>
//        /// <returns></returns>
//        public static IEnumerable<T> FormatTreeData<T, TId>(
//            List<T> source,
//            TId topParentValue,
//            Func<T, TId> getParentIdFunc,
//            Func<T, TId> getCurrIdFunc,
//            Func<T, TId, bool> getCurrFunc,
//            Func<T, TId, bool> getChildrenFunc,
//            Action<T, List<T>> setChildDataFunc,
//            Action<T> toEnd,
//            Action<T, T> level = null)
//             where T : class, new()
//        {
//            var result = new List<T>();
//            var topList = GetTop<T, TId>(source, getCurrIdFunc, getParentIdFunc, topParentValue);
//            foreach (var topId in topList)
//            {
//                var curr = source.FirstOrDefault(d => getCurrFunc(d, topId));

//                //level
//                level?.Invoke(curr, null);

//                FormatChildClass<T, TId>(curr, source, getCurrIdFunc, getChildrenFunc, setChildDataFunc, toEnd, level);
//                result.Add(curr);
//            }
//            return result;
//        }

//        /// <summary>
//        /// 处理子级
//        /// </summary>
//        /// <typeparam name="T">数据源类型</typeparam>
//        /// <typeparam name="TId">主键Id类型</typeparam>
//        /// <param name="curr">当前数据</param>
//        /// <param name="source">原始数据集合</param>
//        /// <param name="getCurrIdFunc">委托：返回主键Id值</param>
//        /// <param name="getChildrenFunc">委托：通过主键值，返回它的下级数据</param>
//        /// <param name="setChildDataFunc">委托：设置当前节点的下级数据</param>
//        /// <param name="toEnd">递归到最底部时执行</param>
//        /// <returns></returns>
//        private static T FormatChildClass<T, TId>(T curr, IEnumerable<T> source, Func<T, TId> getCurrIdFunc, Func<T, TId, bool> getChildrenFunc, Action<T, List<T>> setChildDataFunc, Action<T> toEnd, Action<T, T> level)
//             where T : class, new()
//        {
//            var child = source.Where(d => getChildrenFunc(d, getCurrIdFunc(curr))).ToList();
//            if (child.Count <= 0)
//            {
//                toEnd?.Invoke(curr);
//                return null;
//            }
//            foreach (var item in child)
//            {
//                //level
//                level?.Invoke(item, curr);
//                FormatChildClass(item, source, getCurrIdFunc, getChildrenFunc, setChildDataFunc, toEnd, level);
//            }
//            setChildDataFunc?.Invoke(curr, child);
//            return curr;
//        }

//        #endregion

//        #region Common

//        /// <summary>
//        /// 获取树形结构的 最上层节点Id集合
//        /// </summary>
//        /// <param name="source">原始数据集合</param>
//        /// <param name="idName">id字段名称</param>
//        /// <param name="pidName">父id字段名称</param>
//        /// <param name="topValue">最顶层父节点的值</param>
//        /// <param name="sortName">排序字段名称</param>
//        /// <returns></returns>
//        public static List<string> GetTop(object source, string idName, string pidName, string topValue, string sortName)
//        {
//            var baseArray = JArray.FromObject(source);

//            //获取最顶层项
//            var topIds = !string.IsNullOrWhiteSpace(sortName)
//                ? baseArray.Where(d => d[pidName].Value<string>() == topValue).OrderBy(d => d[sortName]).Select(d => d[idName].Value<string>()).ToList()
//                : baseArray.Where(d => d[pidName].Value<string>() == topValue).Select(d => d[idName].Value<string>()).ToList();

//            var allIds = baseArray.Select(d => d[idName].Value<string>()).Distinct().ToList();
//            ////获取没有最顶层树形的最高层
//            //var dic = allIds.ToDictionary(d => d, d => baseArray.FirstOrDefault(item => item[idName].Value<string>() == d)?[pidName].Value<string>());
//            //var highIds = dic.Where(d => !dic.ContainsKey(d.Value)).Select(d => d.Key).ToList();

//            //topIds.AddRange(highIds);
//            return topIds.Distinct().ToList();
//        }

//        /// <summary>
//        /// 获取树形结构的 最上层节点Id集合
//        /// </summary>
//        /// <typeparam name="T">数据源类型</typeparam>
//        /// <typeparam name="TId">主键Id类型</typeparam>
//        /// <param name="source">数据总集合</param>
//        /// <param name="getIdFunc">委托：返回主键Id值</param>
//        /// <param name="getParentIdFunc">委托，返回父节点id值</param>
//        /// <param name="topValue">最顶层的父节点Id值</param>
//        /// <returns></returns>
//        public static IEnumerable<TId> GetTop<T, TId>(List<T> source, Func<T, TId> getIdFunc, Func<T, TId> getParentIdFunc, TId topValue)
//        {
//            //获取最顶层项
//            var topIds = source.Where(d => getParentIdFunc(d).Equals(topValue)).Select(getIdFunc).ToList();

//            var allIds = source.Select(getIdFunc).Distinct().ToList();
//            //获取没有最顶层树形的最高层
//            var dic = allIds.ToDictionary(d => d, d => getParentIdFunc(source.FirstOrDefault(item => getIdFunc(item).Equals(d))));
//            var highIds = dic.Where(d => !dic.ContainsKey(d.Value)).Select(d => d.Key).ToList();

//            topIds.AddRange(highIds);
//            return topIds.Distinct().ToList();
//        }

//        ///// <summary>
//        ///// 获取树形结构的 给定节点的所有单线上层节点
//        ///// </summary>
//        ///// <typeparam name="T">数据源类型</typeparam>
//        ///// <typeparam name="TId">主键Id类型</typeparam>
//        ///// <param name="source">数据总集合</param>
//        ///// <param name="getIdFunc">委托：返回主键Id值</param>
//        ///// <param name="getParentIdFunc">委托，返回父节点id值</param>
//        ///// <param name="currValue">当前节点值</param>
//        ///// <returns></returns>
//        //public static IEnumerable<TId> GetTopLine<T, TId>(List<T> source, Func<T, TId> getIdFunc, Func<T, TId> getParentIdFunc, TId currValue)
//        //{
//        //    //获取最顶层项
//        //    var topIds = source.Where(d => getParentIdFunc(d).Equals(topValue)).Select(getIdFunc).ToList();

//        //    var allIds = source.Select(getIdFunc).Distinct().ToList();
//        //    //获取没有最顶层树形的最高层
//        //    var dic = allIds.ToDictionary(d => d, d => getParentIdFunc(source.FirstOrDefault(item => getIdFunc(item).Equals(d))));
//        //    var highIds = dic.Where(d => !dic.ContainsKey(d.Value)).Select(d => d.Key).ToList();

//        //    topIds.AddRange(highIds);
//        //    return topIds.Distinct().ToList();
//        //}

//        #endregion
//    }
//}
