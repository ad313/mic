using Mic.Extensions;
using Mic.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mic.Mongo.Extensions
{
    /// <summary>
    /// 查询扩展
    /// </summary>
    public static class MongoCollectionExtensions
    {
        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IFindFluent<T, T> Where<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> where)
            where T : class, new()
        {
            return collection.Find(where);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="where"></param>
        /// <param name="doWhere"></param>
        /// <returns></returns>
        public static IFindFluent<T, T> WhereIf<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> where,
            bool doWhere) where T : class, new()
        {
            return doWhere ? collection.Find(where) : collection.Find(d => true);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IFindFluent<T, T> Where<T>(this IFindFluent<T, T> query, Expression<Func<T, bool>> where)
            where T : class, new()
        {
            if (query.Filter is ExpressionFilterDefinition<T> filter)
            {
                query.Filter = new ExpressionFilterDefinition<T>(filter.Expression.And(where));
            }

            return query;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="where"></param>
        /// <param name="doWhere"></param>
        /// <returns></returns>
        public static IFindFluent<T, T> WhereIf<T>(this IFindFluent<T, T> query, Expression<Func<T, bool>> where,
            bool doWhere) where T : class, new()
        {
            if (!doWhere)
                return query;

            if (query.Filter is ExpressionFilterDefinition<T> filter)
            {
                query.Filter = new ExpressionFilterDefinition<T>(filter.Expression.And(where));
            }

            return query;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static IOrderedFindFluent<T, T> OrderBy<T>(this IFindFluent<T, T> query,
            Expression<Func<T, object>> sort) where T : class, new()
        {
            return query.SortBy(sort);
        }

        /// <summary>
        /// OrderByDescending
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static IOrderedFindFluent<T, T> OrderByDescending<T>(this IFindFluent<T, T> query,
            Expression<Func<T, object>> sort) where T : class, new()
        {
            return query.SortByDescending(sort);
        }

        ///// <summary>
        ///// 分页
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="query"></param>
        ///// <param name="pager"></param>
        ///// <returns></returns>
        //public static async Task<PagerList<T>> ToPagerListAsync<T>(this IFindFluent<T, T> query, Pager pager) where T : class, new()
        //{
        //    var count = await query.CountDocumentsAsync();
        //    var list = await query.Skip((pager.Page - 1) * pager.PageSize).Limit(pager.PageSize).ToListAsync();
        //    return new PagerList<T>(pager, list, count);
        //}

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        public static async Task<PagerList<TResult>> ToPagerListAsync<T, TResult>(this IFindFluent<T, TResult> query,
            Pager pager) where T : class, new()
        {
            var count = await query.CountDocumentsAsync();
            var temp = query.Skip((pager.Page - 1) * pager.PageSize).Limit(pager.PageSize);
            var list = await temp.ToListAsync();
            return new PagerList<TResult>(pager, list, count, temp.ToString());
        }

        /// <summary>
        /// ToList 防止和 System.Linq 方法冲突
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static async Task<List<T>> ToMongoListAsync<T>(this IMongoQueryable<T> query)
        {
            return await query.ToListAsync();
        }
    }
}