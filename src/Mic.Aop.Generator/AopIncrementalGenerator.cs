using Mic.Aop.Generator.MetaData;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using RazorEngineCore;

namespace Mic.Aop.Generator
{
    /// <summary>
    /// 代码生成器
    /// </summary>
    [Generator]
    public class IncrementalGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();

            //IncrementalValuesProvider<AdditionalText> textFiles = context.AdditionalTextsProvider.Where(file => file.Path.EndsWith(".cs"));

            // 找到对什么文件感兴趣
            IncrementalValueProvider<Compilation> compilations =
                context.CompilationProvider
                    // 这里的 Select 是仿照 Linq 写的，可不是真的 Linq 哦，只是一个叫 Select 的方法
                    // public static IncrementalValueProvider<TResult> Select<TSource,TResult>(this IncrementalValueProvider<TSource> source, Func<TSource,CancellationToken,TResult> selector)
                    .Select((compilation, cancellationToken) => compilation);

            context.RegisterSourceOutput(compilations, Execute);
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="compilation"></param>
        public void Execute(SourceProductionContext context, Compilation compilation)
        {
            var classDeclarationSyntax = compilation.SyntaxTrees.SelectMany(d => d.GetRoot(context.CancellationToken)
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()).ToList();

            var interfaceDeclarationSyntax = compilation.SyntaxTrees.SelectMany(d => d.GetRoot(context.CancellationToken)
                .DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>()).ToList();

            if(!classDeclarationSyntax.Any() && !interfaceDeclarationSyntax.Any())
                return;
            
            var sb = new StringBuilder();
            var receiver = new AopSyntaxReceiver(classDeclarationSyntax, interfaceDeclarationSyntax);
            try
            {
                var aopMateData = receiver
                    .FindAopInterceptor()
                    .GetAopMetaData(compilation);

                var builders = aopMateData
                    .GetAopCodeBuilderMetaData()
                    .Select(i => new AopCodeBuilder(i))
                    .Distinct()
                    .ToList();

                foreach (var builder in builders)
                {
                    context.AddSource(builder.SourceCodeName, builder.ToSourceText());
                }

                foreach (var builder in builders)
                {
                    builder.ToTraceStringBuilder(sb);
                }

                context.AddSource("AopClassExtensions", ToRegisterCode(builders, aopMateData).ToString());
                context.AddSource("error", "");
			}
            catch (Exception e)
            {
                context.AddSource("error", ToError(e).ToString());
            }

            context.AddSource("remark", sb.ToString());
        }

        private StringBuilder ToError(Exception e)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"// Message：{e.Message}");
            sb.AppendLine($"// InnerException：{e.InnerException?.Message}");

            var trace = e.StackTrace?.Replace("\r\n", "\r\n//");
            sb.Append($"// StackTrace：{trace}");

            sb.AppendLine();

            var interTrace = e.InnerException?.StackTrace?.Replace("\r\n", "\r\n//");
            sb.Append($"// InnerException.StackTrace：{interTrace}");
            return sb;
        }

        private StringBuilder ToRegisterCode(List<AopCodeBuilder> builders, AopMetaData mateData)
        {
	        var sb = new StringBuilder();

	        var currentAssembly = Assembly.GetExecutingAssembly();
	        var name = currentAssembly.GetName().Name;
	        name += ".Template.RegisterAopClass.cshtml";
			var stream = currentAssembly.GetManifestResourceStream(name);
	        var code = stream.GetString();
            
			IRazorEngine razorEngine = new RazorEngine();
            var template = razorEngine.Compile<AopBuildModel>(code, d =>
            {
				d.AddAssemblyReference(typeof(System.Collections.IList));
				d.AddAssemblyReference(currentAssembly);
            });

            string result = template.Run(d =>
            {
                d.AopMetaDataModel = mateData;
                d.AopCodeBuilderModel = builders;
            });

            sb.Append(result);
            return sb;
        }

		//private StringBuilder ToRegisterCode(List<AopCodeBuilder> builders, AopMetaData mateData)
		//{
		//    var sb = new StringBuilder();
		//    sb.AppendLine("namespace Microsoft.Extensions.DependencyInjection");
		//    sb.AppendLine("{");
		//    sb.AppendLine($"\tinternal static class AopClassExtensions");
		//    sb.AppendLine("\t{");
		//    sb.AppendLine("\t\tpublic static IServiceCollection RegisterAopClass(this IServiceCollection services)");
		//    sb.AppendLine("\t\t{");

		//    foreach (var aopAttribute in mateData.AopAttributeClassMetaDataList)
		//    {
		//        sb.AppendLine($"\t\t\tservices.AddTransient<{aopAttribute.Key}>();");
		//    }

		//    sb.AppendLine();

		//    foreach (var builder in builders)
		//    {
		//        if (builder._metaData.InterfaceMetaData.Any())
		//        {
		//            sb.AppendLine($"\t\t\tservices.AddScoped<{builder._metaData.InterfaceMetaData.First().Key}, {builder._metaData.NameSpace}.{builder.ClassName}>();");
		//        }
		//        else
		//        {
		//            //sb.AppendLine($"\t\t\tservices.AddScoped<{builder._metaData.NameSpace}.{builder.ClassName}>();");
		//            sb.AppendLine($"\t\t\tservices.AddScoped<{builder._metaData.NameSpace}.{builder._metaData.Name}, {builder._metaData.NameSpace}.{builder.ClassName}>();");
		//        }
		//    }

		//    sb.AppendLine("\t\t\treturn services;");
		//    sb.AppendLine("\t\t}");
		//    sb.AppendLine("\t}");
		//    sb.AppendLine("}");

		//    return sb;
		//}
	}

    public class AopBuildModel : RazorEngineTemplateBase
    {
	    public List<AopCodeBuilder> AopCodeBuilderModel { get; set; }

	    public AopMetaData AopMetaDataModel { get; set; }
    }
}
