using Mic.Models;
using Mic.Mongo.Extensions;
using Mic.Mongo.Metadata;
using Mic.Mongo.Models;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Mic.Mongo
{
    /// <summary>
    /// MongoDB 客户端
    /// </summary>
    public static class MgClient
    {
        private static readonly ConcurrentDictionary<Type, SuperTableInfo> SuperTableDic = new ConcurrentDictionary<Type, SuperTableInfo>();

        private static bool _isInited = false;

        private static HostModel HostModel { get; set; }

        private static MongoClient MongoClient { get; set; }

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="host"></param>
        public static void Init(HostModel host)
        {
            if (host == null)
                throw new Exception("MongoDb HostModel 不能为空");

            if (_isInited)
                return;

            _isInited = true;

            MongoClient = new MongoClient(host.GetMongoClientSettings());

            HostModel = HostModel;
        }

        #endregion

        #region 新增

        public static async Task<Result> InsertAsync<T>(T model, string table = null, string database = null, CancellationToken token = default) where T : class, new()
        {
            return await InsertInternal(model, table, database, token).CatchResult(token);
        }

        public static async Task<Result> InsertAsync<T>(List<T> models, string table = null, string database = null, CancellationToken token = default) where T : class, new()
        {
            return await InsertInternal(models, table, database, token).CatchResult(token);
        }

        public static async Task<Result> InsertOrReplaceAsync<T>(T model, Expression<Func<T, bool>> where, string table = null, string database = null, CancellationToken token = default) where T : class, new()
        {
            return await InsertOrReplaceInternal(model, where, table, database, token).CatchResult(token);
        }

        public static async Task<Result> InsertOrReplaceAsync<T>(List<T> model, Expression<Func<T, bool>> where, string table = null, string database = null, CancellationToken token = default) where T : class, new()
        {
            return await InsertOrReplaceInternal(model, where, table, database, token).CatchResult(token);
        }

        #endregion

        #region 删除

        public static async Task<long> DeleteAsync<T>(Expression<Func<T, bool>> where) where T : class, new()
        {
            var tableInfo = ResolverSuperTable<T>();
            var db = MongoClient.GetDatabase(tableInfo.Database);
            var collection = db.GetCollection<T>(tableInfo.TableName);

            return (await collection.DeleteManyAsync(where)).DeletedCount;
        }

        #endregion

        #region 查询

        public static async Task<T> QueryFirstAsync<T>(Expression<Func<T, bool>> where, string table = null, string database = null, CancellationToken token = default) where T : class, new()
        {
            return (await QueryListInternal<T>(where, table, database, token)).FirstOrDefault();
        }

        public static async Task<List<T>> QueryListAsync<T>(Expression<Func<T, bool>> where, string table = null, string database = null, CancellationToken token = default) where T : class, new()
        {
            return await QueryListInternal<T>(where, table, database, token);
        }

        public static IMongoCollection<T> GetCollection<T>(string table = null, string database = null) where T : class, new()
        {
            var tableInfo = ResolverSuperTable<T>();
            var db = MongoClient.GetDatabase(database ?? tableInfo.Database);
            var collection = db.GetCollection<T>(table ?? tableInfo.TableName);
            return collection;
        }

        public static async Task<List<T>> QueryAllListAsync<T>(string table = null, string database = null, CancellationToken token = default) where T : class, new()
        {
            return await QueryListInternal<T>(null, table, database, token);
        }

        #endregion

        #region Internal

        #region 新增

        static async Task<Result> InsertInternal<T>(T model, string table = null, string database = null, CancellationToken cancellationToken = default) where T : class, new()
        {
            var collection = GetCollection<T>(table, database);
            await collection.InsertOneAsync(model, cancellationToken: cancellationToken);
            return new Result(true);
        }

        static async Task<Result> InsertInternal<T>(List<T> models, string table = null, string database = null, CancellationToken cancellationToken = default) where T : class, new()
        {
            var collection = GetCollection<T>(table, database);
            await collection.InsertManyAsync(models, cancellationToken: cancellationToken);
            return new Result(true);
        }

        static async Task<Result> InsertOrReplaceInternal<T>(T model, Expression<Func<T, bool>> where, string table = null, string database = null, CancellationToken cancellationToken = default) where T : class, new()
        {
            var collection = GetCollection<T>(table, database);
            await collection.DeleteOneAsync(where, cancellationToken);
            await collection.InsertOneAsync(model, cancellationToken: cancellationToken);
            return new Result(true);
        }

        static async Task<Result> InsertOrReplaceInternal<T>(List<T> model, Expression<Func<T, bool>> where, string table = null, string database = null, CancellationToken cancellationToken = default) where T : class, new()
        {
            var collection = GetCollection<T>(table, database);
            await collection.DeleteManyAsync(where, cancellationToken);
            await collection.InsertManyAsync(model, cancellationToken: cancellationToken);
            return new Result(true);
        }

        #endregion

        #region 查询

        static async Task<List<T>> QueryListInternal<T>(Expression<Func<T, bool>> where, string table = null, string database = null, CancellationToken cancellationToken = default) where T : class, new()
        {
            var collection = GetCollection<T>(table, database);
            var list = where == null
                ? await collection.Find(d => true).ToListAsync(cancellationToken: cancellationToken)
                : await collection.Find(where).ToListAsync(cancellationToken: cancellationToken);
            return list;
        }

        #endregion

        #endregion

        #region private

        private static SuperTableInfo ResolverSuperTable<T>() where T : class, new()
        {
            var type = typeof(T);
            if (SuperTableDic.TryGetValue(type, out SuperTableInfo superTable))
                return superTable;

            superTable = new SuperTableInfo(type, HostModel);
            SuperTableDic.TryAdd(type, superTable);
            return superTable;
        }

        #endregion
    }
}
