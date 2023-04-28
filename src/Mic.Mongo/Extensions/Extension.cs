using Mic.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mic.Mongo.Extensions
{
    public static class Extension
    {
        /// ExecuteNoQuery 处理
        public static async Task<Result> CatchResult(this Task<int> action, CancellationToken cancellationToken = default)
        {
            return await action.ContinueWith(task => task.IsCompletedSuccessfully ? new Result(task.Result) : new Result(task.Exception), cancellationToken);
        }
        
        /// Result<List<T>> 处理
        public static async Task<Result<List<T>>> CatchResult<T>(this Task<Result<List<T>>> action, CancellationToken cancellationToken = default) where T : class, new()
        {
            return await action.ContinueWith(task =>
                task.IsCompletedSuccessfully ?
                    task.Result :
                    new Result<List<T>>(task.Exception), cancellationToken);
        }

        /// Result 处理
        public static async Task<Result> CatchResult(this Task<Result> action, CancellationToken cancellationToken = default)
        {
            return await action.ContinueWith(task =>
                task.IsCompletedSuccessfully ?
                    task.Result :
                    new Result(task.Exception), cancellationToken);
        }
        
        /// Scalar 处理
        public static async Task<ScalarResult<List<T>>> CatchResult<T>(this Task<ScalarResult<List<T>>> action, CancellationToken cancellationToken = default)
        {
            return await action.ContinueWith(task =>
                task.IsCompletedSuccessfully ?
                    task.Result :
                    new ScalarResult<List<T>>(task.Exception), cancellationToken);
        }
    }
}