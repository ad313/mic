using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Mic.BenchmarkDotNet.PoolCache
{
    public class CreateInstanceHelper
    {
        private Func<object> _creator = null;

        public T CreateInstance<T>()
        {
            _creator ??= CreateInstanceFunc(typeof(T));

            return (T)_creator.Invoke();
        }
        
        private Func<object> CreateInstanceFunc(Type type)
        {
            Func<object> _creator;

            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var classTypeInfo = type.GetTypeInfo();
            var defaultConstructor = classTypeInfo
                .GetConstructors(bindingFlags)
                .SingleOrDefault(c => c.GetParameters().Length == 0);
            if (defaultConstructor != null)
            {
                _creator = () => defaultConstructor.Invoke(null);
            }
            else
            {
                _creator = () => FormatterServices.GetUninitializedObject(type);
            }

            return _creator;
        }
    }
}
