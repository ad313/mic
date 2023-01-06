using Mic.Aop.Generator.MetaData;
using Mic.Aop.Generator.Renders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mic.Aop.Generator
{
    /// <summary>
    /// 代码生成器
    /// </summary>
    [Generator]
    public class IncrementalGenerator : IIncrementalGenerator
    {
        private readonly Assembly _currentAssembly = Assembly.GetExecutingAssembly();

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
                Execute(context, compilation.Left, compilation.Right);
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

            var classDeclarationSyntax = compilation.SyntaxTrees.SelectMany(d => d.GetRoot(context.CancellationToken)
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()).ToList();

            var interfaceDeclarationSyntax = compilation.SyntaxTrees.SelectMany(d => d.GetRoot(context.CancellationToken)
                .DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>()).ToList();

            if (!classDeclarationSyntax.Any() && !interfaceDeclarationSyntax.Any())
                return;

            if (context.CancellationToken.IsCancellationRequested) return;

            AssemblyMetaData meta = null;
            var receiver = new AopSyntaxReceiver(classDeclarationSyntax, interfaceDeclarationSyntax);
            try
            {
                meta = receiver
                    .FindAopInterceptors()
                    .GetMetaData(compilation);

                var builders = meta
                    .GetAopCodeBuilderMetaData()
                    .Select(i => new AopCodeBuilder(i))
                    .Distinct()
                    .ToList();

                if (context.CancellationToken.IsCancellationRequested)
                    return;

                BuildAop(context, meta, builders);
            }
            catch (Exception e)
            {
                context.AddSource("Error", TemplateRender.ToError(_currentAssembly, e).ToString());
            }

            watch.Stop();

            var timesBuilder = new StringBuilder();
            timesBuilder.AppendLine($"//Aop：{DateTime.Now} - {watch.ElapsedMilliseconds} 毫秒");
            watch.Restart();

            BuildExtend(context, additionalTexts, meta);

            watch.Stop();
            timesBuilder.AppendLine($"//BuildExtend：{DateTime.Now} - {watch.ElapsedMilliseconds} 毫秒");
            context.AddSource("Times", timesBuilder.ToString());
        }

        /// <summary>
        /// 构建Aop代码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="meta"></param>
        /// <param name="builders"></param>
        private void BuildAop(SourceProductionContext context, AssemblyMetaData meta, List<AopCodeBuilder> builders)
        {
            foreach (var builder in builders)
            {
                context.AddSource(builder.SourceCodeName, builder.ToSourceText());
            }

            context.AddSource("Remark", TemplateRender.ToTrace(_currentAssembly, builders, meta).ToString());
            context.AddSource("AopClassExtensions", TemplateRender.ToRegisterCode(_currentAssembly, builders, meta).ToString());
            context.AddSource("Error", "");
        }

        /// <summary>
        /// 构建扩展代码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="additionalTexts"></param>
        /// <param name="meta"></param>
        private void BuildExtend(SourceProductionContext context, ImmutableArray<AdditionalText> additionalTexts, AssemblyMetaData meta)
        {
            try
            {
                var extendMapModels = GetExtendMapModels(additionalTexts);
                extendMapModels.ForEach(item => item.AopMetaDataModel = meta);
                RenderExtend(context, meta, extendMapModels);

                context.AddSource("BuildExtendError", "");
            }
            catch (Exception e)
            {
                context.AddSource("BuildExtendError", TemplateRender.ToError(_currentAssembly, e).ToString());
            }
        }

        private List<ExtendTemplateModel> GetExtendMapModels(ImmutableArray<AdditionalText> additionalTexts)
        {
            var mapText = additionalTexts.FirstOrDefault(d => d.Path.EndsWith("Map.txt", StringComparison.OrdinalIgnoreCase))?.GetText()?.ToString();
            if (string.IsNullOrWhiteSpace(mapText))
                return new List<ExtendTemplateModel>();

            return Regex.Split(mapText, "#")
                 .Select(txt =>
                 {
                     if (string.IsNullOrWhiteSpace(txt))
                         return null;

                     var arr = Regex.Split(txt.Trim(), "\r\n");
                     if (arr.Length < 4)
                         return null;

                     var model = new ExtendTemplateModel();
                     foreach (var line in arr)
                     {
                         if (line.IndexOf(":", StringComparison.OrdinalIgnoreCase) <= -1 || line.StartsWith("//"))
                             continue;

                         var split = line.Split(':');
                         switch (split[0].ToLower())
                         {
                             case "type":
                                 model.Type = (ExtendTemplateType)(int.TryParse(split[1], out int v) ? v : 0);
                                 break;
                             case "name":
                                 model.Name = split[1];
                                 break;
                             case "code":
                                 model.Code = split[1];
                                 break;
                             case "templates":
                                 model.Templates = split[1].Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => d.Trim()).ToList();
                                 break;
                             default:
                                 break;
                         }
                     }

                     if (model.Templates.Any())
                     {
                         foreach (var template in model.Templates)
                         {
                             var file = additionalTexts.FirstOrDefault(d => d.Path.EndsWith($"\\{template}", StringComparison.OrdinalIgnoreCase));
                             if (file == null) continue;

                             model.TemplateDictionary ??= new ConcurrentDictionary<string, string>();
                             model.TemplateDictionary.TryAdd(template, file.GetText()?.ToString());
                         }
                     }

                     return model;
                 }).Where(d => d != null).ToList();
        }

        private void RenderExtend(SourceProductionContext context, AssemblyMetaData meta, List<ExtendTemplateModel> extendMapModels)
        {
            foreach (var extendMapModel in extendMapModels)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;

                switch (extendMapModel.Type)
                {
                    case ExtendTemplateType.ClassTarget:
                        RenderExtendByClassMetaData(context, meta, extendMapModel);
                        break;
                    case ExtendTemplateType.InterfaceTarget:
                        RenderExtendByInterfaceMetaData(context, meta, extendMapModel);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void RenderExtendByClassMetaData(SourceProductionContext context, AssemblyMetaData meta, ExtendTemplateModel extendMapModel)
        {
            foreach (var classMetaData in meta.ClassMetaDataList)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;

                extendMapModel.ClassMetaData = classMetaData;

                TemplateRender.RenderExtend(extendMapModel, d =>
                {
                    d.AddAssemblyReference(typeof(System.Collections.IList));
                    d.AddAssemblyReference(typeof(System.Linq.Enumerable));
                    d.AddAssemblyReference(typeof(System.Linq.IQueryable));
                    d.AddAssemblyReference(typeof(System.Collections.Generic.IEnumerable<>));
                    d.AddAssemblyReference(typeof(System.Collections.Generic.List<>));
                    d.AddAssemblyReference(typeof(System.Diagnostics.Debug));
                    d.AddAssemblyReference(typeof(System.Diagnostics.CodeAnalysis.SuppressMessageAttribute));
                    d.AddAssemblyReference(typeof(System.Runtime.Versioning.ComponentGuaranteesAttribute));
                    d.AddAssemblyReference(typeof(System.Linq.Expressions.BinaryExpression));

                    d.AddAssemblyReference(_currentAssembly);
                }, d =>
                {
                    d.Model = extendMapModel;
                });

                foreach (var kv in extendMapModel.TemplateResult)
                {
                    context.AddSource($"{kv.Key.Replace(".txt", "")}.cs", kv.Value);
                }

                extendMapModel.TemplateResult.Clear();
            }
        }

        private void RenderExtendByInterfaceMetaData(SourceProductionContext context, AssemblyMetaData meta, ExtendTemplateModel extendMapModel)
        {
            foreach (var interfaceMetaData in meta.InterfaceMetaDataList)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;

                extendMapModel.InterfaceMetaData = interfaceMetaData;

                TemplateRender.RenderExtend(extendMapModel, d =>
                {
                    d.AddAssemblyReference(typeof(System.Collections.IList));
                    d.AddAssemblyReference(typeof(Enumerable));
                    d.AddAssemblyReference(_currentAssembly);
                }, d =>
                {
                    d.Model = extendMapModel;
                });

                foreach (var kv in extendMapModel.TemplateResult)
                {
                    context.AddSource($"{kv.Key.Replace(".txt", "")}.cs", kv.Value);
                }

                extendMapModel.TemplateResult.Clear();
            }
        }
    }
}
