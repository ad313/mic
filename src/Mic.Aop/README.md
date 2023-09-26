## Aop

### 使用方法
#### 1、定义 Attribute，继承 AopInterceptor
    /// <summary>
    /// 日志
    /// </summary>
    public class LogAttribute : AopInterceptor
    {
        /// <summary>
        /// 日志服务，只有 实际方法、After
        /// </summary>
        public LogAttribute()
        {
            HasBefore = false;
            HasAopNext = false;
        }

        public override AopContext After(AopContext context)
        {
            //Console.WriteLine("log trace sync");
            return context;
        }
        
        /// <summary>执行后操作，异步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValueTask<AopContext> AfterAsync(AopContext context)
        {
            //Console.WriteLine("log trace async");
            return base.AfterAsync(context);
        }
    }

#### 2、注入依赖关系
    services.RegisterAopClass();
        
#### 3、正常使用，注意：实际的方法必须是可重写的，即加了 override 或 virtual
    在接口或类上打标签
    
### 实现原理
#### 1、一共有6个方法，分别是“执行前、执行实际方法、执行后”三个方法的同步和异步版本。同步和异步分别调用对应的方法。
    /// <summary>
    /// Aop 拦截器
    /// </summary>
    public interface IAopInterceptor
    {
        /// <summary>
        /// 执行前操作，同步方法调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        AopContext Before(AopContext context);
        /// <summary>
        /// 执行前操作，异步方法调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        ValueTask<AopContext> BeforeAsync(AopContext context);
        /// <summary>
        /// 执行后操作，同步方法调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        AopContext After(AopContext context);
        /// <summary>
        /// 执行后操作，异步方法调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        ValueTask<AopContext> AfterAsync(AopContext context);
        /// <summary>
        /// 执行方法，同步方法调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        AopContext Next(AopContext context);
        /// <summary>
        /// 执行方法，异步方法调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        ValueTask<AopContext> NextAsync(AopContext context);
    }
#### 2、定义接口和实现类。（必须加上 AopTag 属性，不管是true还是false，都会触发生成代码）
    public interface ITestService
    {
        [Log(AopTag = true)]
        string HasReturnSync();
        
        void NoReturnSync(string name, int id);

        Task<string> HasReturnAsync(string name);
        
        Task<string> HasReturnAsync(string name, int aaa);
    }
    
    
    public class TestService : ITestService
    {
        public TestService()
        {
        }

        public virtual string HasReturnSync()
        {
            //Console.WriteLine($"method: GetNameSync");
            return "ad313";
        }

        [Log]
        public virtual void NoReturnSync(string name, int id)
        {
            //Console.WriteLine($"method:set name:{name}");
        }

        public virtual Task<string> HasReturnAsync(string name)
        {
            //Console.WriteLine("excuter.....");
            return Task.FromResult($"ad313 async:{name}");
        }

        [Log]
        public virtual async Task<string> HasReturnAsync(string name, int aaa)
        {
            return await Task.FromResult($"ad313_2 async:{name}");
        }
    }
    
#### 3、通过 SourceGenerator 自动生成
    public sealed class TestService_g : TestService
	{
		private readonly IServiceProvider _serviceProvider0;
		public TestService_g(IServiceProvider serviceProvider0)
		{
			_serviceProvider0 = serviceProvider0;
		}

		public override void NoReturnSync(string name, int id)
		{
			var aopContext = new AopContext(_serviceProvider0,
				new Dictionary<string, dynamic>() { { "name", name }, { "id", id } },
				false,
				false,
				null);

			var aopInterceptor0 = _serviceProvider0.GetRequiredService<LogAttribute>();
			if (aopInterceptor0.HasBefore) aopContext = aopInterceptor0.Before(aopContext);
			if (aopInterceptor0.HasAopNext)
			{
				if (aopInterceptor0.HasActualNext)
				{
					aopContext.ActualMethod = () => Task.Run(() => base.NoReturnSync(name, id));
				}
				aopContext = aopInterceptor0.Next(aopContext);
			}
			else
			{
				if (aopInterceptor0.HasActualNext)
				{
					base.NoReturnSync(name, id);
				}
			}
			if (aopInterceptor0.HasAfter) aopContext = aopInterceptor0.After(aopContext);
		}

		public override async Task<string> HasReturnAsync(string name, int aaa)
		{
			var aopContext = new AopContext(_serviceProvider0,
				new Dictionary<string, dynamic>() { { "name", name }, { "aaa", aaa } },
				true,
				true,
				null);

			var aopInterceptor0 = _serviceProvider0.GetRequiredService<LogAttribute>();
			if (aopInterceptor0.HasBefore) aopContext = await aopInterceptor0.BeforeAsync(aopContext);
			if (aopInterceptor0.HasAopNext)
			{
				if (aopInterceptor0.HasActualNext)
				{
					aopContext.ActualMethod = () => base.HasReturnAsync(name, aaa);
				}
				aopContext = await aopInterceptor0.NextAsync(aopContext);
			}
			else
			{
				if (aopInterceptor0.HasActualNext)
				{
					aopContext.ReturnValue = await base.HasReturnAsync(name, aaa);
				}
			}
			if (aopInterceptor0.HasAfter) aopContext = await aopInterceptor0.AfterAsync(aopContext);

			return aopContext.ReturnValue;
		}

	}
#### 4、一些优先级和规则
##### 1、优先级：就近原则，类方法上的标签 > 类上的标签 > 接口方法上的标签 > 接口上的标签
##### 2、忽略Aop：打上 [IgnoreAop] 标签
##### 3、如果一个方法打上多个Attribute，则按照管道的原则，先进后出，注意，只有最接近方法的 Attribute 才能调用 Next 方法。如果有 三个 Attribute，分别是 attribute1、attribute2、attribute3，则执行顺序是 attribute1.Before => attribute2.Before => attribute3.Before => attribute3.Next => attribute3.After => attribute2.After => attribute1.After
##### 4、自定义执行方法：内置了 4个属性 
        /// <summary>
        /// 是否执行 Before
        /// </summary>
        public bool HasBefore { get; set; }
        /// <summary>
        /// 是否执行 After
        /// </summary>
        public bool HasAfter { get; set; }
        /// <summary>
        /// 是否执行 Aop 的 Next
        /// </summary>
        public bool HasAopNext { get; set; }
        /// <summary>
        /// 是否执行实际的方法
        /// </summary>
        public bool HasActualNext { get; set; }
      
##### 根据使用场景，如果是日志，那么只有 “实际方法” 和 “After” ，就可以把另外两个设置为false，就不会调用另外两个方法：如下代码：
    /// <summary>
    /// 日志
    /// </summary>
    public class LogAttribute : AopInterceptor
    {
        /// <summary>
        /// 日志服务，只有 实际方法、After
        /// </summary>
        public LogAttribute()
        {
            HasBefore = false;
            HasAopNext = false;
        }

        public override AopContext After(AopContext context)
        {
            //Console.WriteLine("log trace sync");
            return context;
        }
        
        /// <summary>执行后操作，异步方法调用</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValueTask<AopContext> AfterAsync(AopContext context)
        {
            //Console.WriteLine("log trace async");
            return base.AfterAsync(context);
        }
    }
      