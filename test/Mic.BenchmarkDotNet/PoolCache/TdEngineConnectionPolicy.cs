using Mic.BenchmarkDotNet;
using Microsoft.Extensions.ObjectPool;

namespace SaiLing.Modules.Datas.TdEngine.Pool
{
    public class TestPolicy : IPooledObjectPolicy<ItemModel>
    {
        public TestPolicy()
        {
        }

        /// <summary>
        /// Create a <typeparamref name="T" />.
        /// </summary>
        /// <returns>The <typeparamref name="T" /> which was created.</returns>
        public ItemModel Create()
        {
            return new ItemModel();
        }

        /// <summary>
        /// Runs some processing when an object was returned to the pool. Can be used to reset the state of an object and indicate if the object should be returned to the pool.
        /// </summary>
        /// <param name="obj">The object to return to the pool.</param>
        /// <returns><code>true</code> if the object should be returned to the pool. <code>false</code> if it's not possible/desirable for the pool to keep the object.</returns>
        public bool Return(ItemModel obj)
        {
            

            return true;
        }
    }
}
