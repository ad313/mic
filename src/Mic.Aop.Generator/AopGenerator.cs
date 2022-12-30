//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using Mic.Aop.Generator.MetaData;
//using Microsoft.CodeAnalysis;
//using RazorEngineCore;

//namespace Mic.Aop.Generator
//{
//    /// <summary>
//    /// 代码生成器
//    /// </summary>
//    [Generator]
//    public class AopGenerator : ISourceGenerator
//    {
//        /// <summary>
//        /// 初始化
//        /// </summary>
//        /// <param name="context"></param>
//        public void Initialize(GeneratorInitializationContext context)
//        {
//            //            Debugger.Launch();






//            //             string Content = @"
//            //Hello @Model.Name

//            //@foreach(var item in @Model.Items)
//            //{
//            //    <div>- @item</div>
//            //}

//            //<div data-name=""@Model.Name""></div>

//            //<area>
//            //    @{ RecursionTest(3); }
//            //</area>

//            //@{
//            //	void RecursionTest(int level){
//            //		if (level <= 0)
//            //		{
//            //			return;
//            //		}

//            //		<div>LEVEL: @level</div>
//            //		@{ RecursionTest(level - 1); }
//            //	}
//            //}";



//            //             IRazorEngine razorEngine = new RazorEngine();
//            //             IRazorEngineCompiledTemplate template = razorEngine.Compile(Content);

//            //             string result = template.Run(new
//            //             {
//            //                 Name = "Alexander",
//            //                 Items = new List<string>()
//            //                 {
//            //                     "item 1",
//            //                     "item 2"
//            //                 }
//            //             });








//            context.RegisterForSyntaxNotifications(() => new AopSyntaxReceiver());
//        }

//        /// <summary>
//        /// 执行
//        /// </summary>
//        /// <param name="context"></param>
//        public void Execute(GeneratorExecutionContext context)
//        {
//            var sb = new StringBuilder();
//            if (context.SyntaxReceiver is AopSyntaxReceiver receiver)
//            {
//                try
//                {
//                    var aopMateData = receiver
//                        .FindAopInterceptor()
//                        .GetAopMetaData(context.Compilation);

//                    var builders = aopMateData
//                        .GetAopCodeBuilderMetaData()
//                        .Select(i => new AopCodeBuilder(i))
//                        .Distinct()
//                        .ToList();

//                    foreach (var builder in builders)
//                    {
//                        context.AddSource(builder.SourceCodeName, builder.ToSourceText());
//                    }

//                    foreach (var builder in builders)
//                    {
//                        builder.ToTraceStringBuilder(sb);
//                    }

//                    context.AddSource("AopClassExtensions", ToRegisterCode(builders, aopMateData).ToString());
//                }
//                catch (Exception e)
//                {
//                    context.AddSource("error", ToError(e).ToString());
//                }
//            }

//            context.AddSource("remark", sb.ToString());
//        }

//        private StringBuilder ToError(Exception e)
//        {
//            var sb = new StringBuilder();
//            sb.AppendLine($"// Message：{e.Message}");
//            sb.AppendLine($"// InnerException：{e.InnerException?.Message}");

//            var trace = e.StackTrace?.Replace("\r\n", "\r\n//");
//            sb.Append($"// StackTrace：{trace}");

//            sb.AppendLine();

//            var interTrace = e.InnerException?.StackTrace?.Replace("\r\n", "\r\n//");
//            sb.Append($"// InnerException.StackTrace：{interTrace}");
//            return sb;
//        }

//        private StringBuilder ToRegisterCode(List<AopCodeBuilder> builders, AopMetaData mateData)
//        {
//            var sb = new StringBuilder();
//            sb.AppendLine("namespace Microsoft.Extensions.DependencyInjection");
//            sb.AppendLine("{");
//            sb.AppendLine($"\tinternal static class AopClassExtensions");
//            sb.AppendLine("\t{");
//            sb.AppendLine("\t\tpublic static IServiceCollection RegisterAopClass(this IServiceCollection services)");
//            sb.AppendLine("\t\t{");

//            foreach (var aopAttribute in mateData.AopAttributeClassMetaDataList)
//            {
//                sb.AppendLine($"\t\t\tservices.AddTransient<{aopAttribute.Key}>();");
//            }

//            sb.AppendLine();

//            foreach (var builder in builders)
//            {
//                if (builder._metaData.InterfaceMetaData.Any())
//                {
//                    sb.AppendLine($"\t\t\tservices.AddScoped<{builder._metaData.InterfaceMetaData.First().Key}, {builder._metaData.NameSpace}.{builder.ClassName}>();");
//                }
//                else
//                {
//                    //sb.AppendLine($"\t\t\tservices.AddScoped<{builder._metaData.NameSpace}.{builder.ClassName}>();");
//                    sb.AppendLine($"\t\t\tservices.AddScoped<{builder._metaData.NameSpace}.{builder._metaData.Name}, {builder._metaData.NameSpace}.{builder.ClassName}>();");
//                }
//            }

//            sb.AppendLine("\t\t\treturn services;");
//            sb.AppendLine("\t\t}");
//            sb.AppendLine("\t}");
//            sb.AppendLine("}");

//            return sb;
//        }
//    }
//}
