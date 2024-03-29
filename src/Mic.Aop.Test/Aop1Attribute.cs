﻿namespace Mic.Aop.Test
{
    /// <summary>
    /// 继承基类，重写相关方法
    /// </summary>
    public class Aop1Attribute : AopInterceptor
    {
        /// <summary>
        /// 定义一个参数
        /// </summary>
        public string Prop1 { get; set; }

        public override ValueTask<AopContext> BeforeAsync(AopContext context)
        {
            Console.WriteLine("BeforeAsync action...");

            return base.BeforeAsync(context);
        }

        public override ValueTask<AopContext> AfterAsync(AopContext context)
        {
            Console.WriteLine("AfterAsync action...");

            return base.AfterAsync(context);
        }
    }


    public class SampleService
    {
        /// <summary>
        /// 必须加上 AopTag 属性，不管是true还是false，都会触发生成代码
        /// 在分析器 - SourceGenerator.Template.Generators 下面可以看到生成的代码，名称是 AopExtend_SampleService_g
        /// 生成了 SampleService 的子类，注入的时候，请注入子类
        /// </summary>
        [Aop1(AopTag = true, Prop1 = "传一个值进去")]
        public virtual void TestAsync()
        {
            Console.WriteLine("Test action...");
        }
    }
}