using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using Assert = NUnit.Framework.Assert;

namespace Mic.Test.Mic.LazyLoader
{
    public class MicLazyLoaderUnitTest
    {
        public IServiceProvider ServiceProvider { get; set; }

        [OneTimeSetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddScoped<LazyLoaderClass>();
            services.AddLazyLoader();

            ServiceProvider = services.BuildServiceProvider();
        }

        [Test]
        public void Test1()
        {
            using var scope = ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<Lazy<LazyLoaderClass>>();

            Assert.IsFalse(service.IsValueCreated);

            var newService = scope.ServiceProvider.GetService<LazyLoaderClass>();
            Assert.IsTrue(service.Value == newService);
        }
    }

    public class LazyLoaderClass
    {
        public string Id { get; set; }

        public LazyLoaderClass()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}