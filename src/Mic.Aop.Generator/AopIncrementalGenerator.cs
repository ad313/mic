using Mic.Aop.Generator.MetaData;
using Mic.Aop.Generator.Renders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mic.Aop.Generator
{
    /// <summary>
    /// 代码生成器
    /// </summary>
    [Generator]
    public class IncrementalGenerator : IIncrementalGenerator
    {
        public static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
        private readonly StringBuilder _errorBuilder = new StringBuilder();
        private readonly StringBuilder _timeBuilder = new StringBuilder();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();

            var textFiles = context.AdditionalTextsProvider.Where(file => file.Path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)).Collect();

            // 找到对什么文件感兴趣
            IncrementalValueProvider<Compilation> compilations =
                context.CompilationProvider
                    // 这里的 Select 是仿照 Linq 写的，可不是真的 Linq 哦，只是一个叫 Select 的方法
                    // public static IncrementalValueProvider<TResult> Select<TSource,TResult>(this IncrementalValueProvider<TSource> source, Func<TSource,CancellationToken,TResult> selector)
                    .Select((compilation, cancellationToken) => compilation);

            context.RegisterSourceOutput(compilations.Combine(textFiles), (context, compilation) =>
            {
                try
                {
                    Execute(context, compilation.Left, compilation.Right);
                }
                catch (Exception e)
                {
                    TemplateRender.ToErrorStringBuilder("全局异常", _errorBuilder, e);
                }
                finally
                {
                    context.AddSource("Error", _errorBuilder.ToString());
                    context.AddSource("Times", _timeBuilder.ToString());
                }
            });
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="compilation"></param>
        /// <param name="additionalTexts">附加文本文件</param>
        public void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<AdditionalText> additionalTexts)
        {
            var watch = Stopwatch.StartNew();
            watch.Start();

            var syntaxNodes = compilation.SyntaxTrees.SelectMany(d => d.GetRoot(context.CancellationToken).DescendantNodes()).ToList();
            var classDeclarationSyntax = syntaxNodes.OfType<ClassDeclarationSyntax>().ToList();
            var structDeclarationSyntax = syntaxNodes.OfType<StructDeclarationSyntax>().ToList();
            var interfaceDeclarationSyntax = syntaxNodes.OfType<InterfaceDeclarationSyntax>().ToList();
            var recordDeclarationSyntax = syntaxNodes.OfType<RecordDeclarationSyntax>().ToList();

            if (!classDeclarationSyntax.Any() && !interfaceDeclarationSyntax.Any()) return;
            if (context.CancellationToken.IsCancellationRequested) return;
            
            var receiver = new AopSyntaxReceiver(classDeclarationSyntax, interfaceDeclarationSyntax);
            AssemblyMetaData meta = null;
            List<AopCodeBuilder> builders;

            #region 1、获取元数据

            try
            {
                meta = receiver
                    .FindAopInterceptors()
                    .GetMetaData(compilation);

                builders = meta
                    .GetAopCodeBuilderMetaData()
                    .Select(i => new AopCodeBuilder(i))
                    .Distinct()
                    .ToList();

                TemplateRender.ToTimeStringBuilder("1、获取元数据", _timeBuilder, watch);
            }
            catch (Exception e)
            {
                TemplateRender.ToErrorStringBuilder("1、获取元数据", _errorBuilder, e);
                return;
            }
            finally
            {
                watch.Restart();
            }

            #endregion

            #region 2、生成Aop代码
            try
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;

                foreach (var builder in builders)
                {
                    context.AddSource(builder.SourceCodeName, builder.ToSourceText());
                }

                TemplateRender.ToTimeStringBuilder("2、生成Aop代码", _timeBuilder, watch);
            }
            catch (Exception e)
            {
                TemplateRender.ToErrorStringBuilder("2、生成Aop代码", _errorBuilder, e);
                return;
            }
            finally
            {
                watch.Restart();
            }
            #endregion

            #region 3、生成Aop Trace
            try
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;

                var traceBuilder = new StringBuilder();

                foreach (var builder in builders)
                {
                    TemplateRender.ToTraceStringBuilder(traceBuilder, builder);
                }

                TemplateRender.ToTimeStringBuilder("3、生成Aop Trace", _timeBuilder, watch);

                context.AddSource("Trace", traceBuilder.ToString());
            }
            catch (Exception e)
            {
                TemplateRender.ToErrorStringBuilder("3、生成Aop Trace", _errorBuilder, e);
                return;
            }
            finally
            {
                watch.Restart();
            }
            #endregion

            #region 4、生成 Register Code 注入服务扩展
            try
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;

                var registerStringBuilder = TemplateRender.ToRegisterStringBuilder(null, builders, meta);
                TemplateRender.ToTimeStringBuilder("4、生成 Register Code 注入服务扩展", _timeBuilder, watch);

                context.AddSource("AopClassExtensions", registerStringBuilder.ToString());
            }
            catch (Exception e)
            {
                TemplateRender.ToErrorStringBuilder("4、生成 Register Code 注入服务扩展", _errorBuilder, e);
                return;
            }
            finally
            {
                watch.Restart();
            }
            #endregion

            #region 5、自定义模板生成代码
            try
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;

                TemplateRender.BuildExtend(context, additionalTexts, meta, _errorBuilder);
                TemplateRender.ToTimeStringBuilder("5、自定义模板生成代码", _timeBuilder, watch);
            }
            catch (Exception e)
            {
                TemplateRender.ToErrorStringBuilder("5、自定义模板生成代码", _errorBuilder, e);
                return;
            }
            finally
            {
                watch.Restart();
            } 
            #endregion
        }
    }
}
