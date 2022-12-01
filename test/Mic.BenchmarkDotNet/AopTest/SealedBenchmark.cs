using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mic.BenchmarkDotNet.AopTest
{
    [MemoryDiagnoser]
    public class SealedBenchmark
    {
        readonly NonSealedType nonSealedType = new();
        readonly SealedType sealedType = new();

        [Benchmark(Baseline = true)]
        public void NonSealed()
        {
            // The JIT cannot know the actual type of nonSealedType. Indeed,
            // it could have been set to a derived class by another method.
            // So, it must use a virtual call to be safe.
            nonSealedType.Method();
        }

        [Benchmark]
        public void Sealed()
        {
            // The JIT is sure sealedType is a SealedType. As the class is sealed,
            // it cannot be an instance from a derived type.
            // So it can use a direct call which is faster.
            sealedType.Method();
        }
    }

    internal class BaseType
    {
        public virtual void Method() { }
    }
    internal class NonSealedType : BaseType
    {
        public override void Method() { }
    }
    internal sealed class SealedType : BaseType
    {
        public override void Method() { }
    }
}
