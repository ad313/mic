using Mic.BenchmarkDotNet.AopTest.MicAop;
using Microsoft.Extensions.ObjectPool;

namespace Mic.BenchmarkDotNet.AopTest.Pool
{
    public class LogAttributePolicy : IPooledObjectPolicy<LogAttribute>
    {
        /// <summary>
        /// Create a <typeparamref name="T" />.
        /// </summary>
        /// <returns>The <typeparamref name="T" /> which was created.</returns>
        public LogAttribute Create()
        {
            return new LogAttribute() { };
        }

        /// <summary>
        /// Runs some processing when an object was returned to the pool. Can be used to reset the state of an object and indicate if the object should be returned to the pool.
        /// </summary>
        /// <param name="obj">The object to return to the pool.</param>
        /// <returns><code>true</code> if the object should be returned to the pool. <code>false</code> if it's not possible/desirable for the pool to keep the object.</returns>
        public bool Return(LogAttribute obj)
        {
           obj.Clear();

            return true;
        }
    }

    public static class PollHelper
    {
        private static LogAttributePolicy LogPolicy { get; set; }
        public static DefaultObjectPool<LogAttribute> LogAttributePolicyPool { get; set; }

        public static void Init()
        {
            LogPolicy = new LogAttributePolicy();
            LogAttributePolicyPool = new DefaultObjectPool<LogAttribute>(LogPolicy,100);
        }
    }
}
