using Microsoft.Extensions.DependencyInjection;
using System;

namespace Mic.LazyLoader
{
    public class LazyLoader<T> : Lazy<T>
    {
        public LazyLoader(IServiceProvider sp) : base(sp.GetRequiredService<T>)
        {
        }
    }
}
