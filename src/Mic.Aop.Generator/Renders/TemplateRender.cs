using Mic.Aop.Generator.MetaData;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mic.Aop.Generator.Renders
{
    internal class TemplateRender
    {
        public static void ToErrorStringBuilder(string name, StringBuilder sb, Exception e)
        {
            sb ??= new StringBuilder();
            sb.AppendLine($"// {name} 异常 =>");
            sb.AppendLine($"// Message：{e.Message?.Replace("\r\n", "\r\n//")}");
            sb.AppendLine($"// InnerException：{e.InnerException?.Message?.Replace("\r\n", "\r\n//")}");
            sb.Append($"// StackTrace：{e.StackTrace?.Replace("\r\n", "\r\n//")}");
            sb.AppendLine();
            sb.Append($"// InnerException.StackTrace：{e.InnerException?.StackTrace?.Replace("\r\n", "\r\n//")}");
            sb.AppendLine();
            sb.AppendLine();
        }

        public static void ToTimeStringBuilder(string name, StringBuilder sb, Stopwatch watch)
        {
            sb ??= new StringBuilder();
            sb.AppendLine($"// {name} =>");
            sb.AppendLine($"// 耗时：{watch.ElapsedMilliseconds}");
            watch.Restart();
            sb.AppendLine();
            sb.AppendLine();
        }

        public static void ToTraceStringBuilder(StringBuilder sb, AopCodeBuilder builder)
        {
            sb ??= new StringBuilder();
            sb.AppendLine($"// -------------------------------------------------");
            sb.AppendLine($"//命名空间：{builder.Namespace}");
            sb.AppendLine($"//文件名称：{builder.SourceCodeName}");
            sb.AppendLine($"//接口：{string.Join("、", builder._metaData.InterfaceMetaData.Select(d => d.Key))}");
            sb.AppendLine($"//类名：{builder.ClassName}");

            foreach (var methodData in builder._metaData.MethodMetaDatas)
            {
                sb.AppendLine();
                sb.AppendLine($"//方法名称：{methodData.Name}");
                sb.AppendLine($"//Key名称：{methodData.Key}");
                sb.AppendLine($"//Aop属性：{string.Join("、", methodData.AttributeMetaData.Select(d => d.Name))}");
            }

            sb.AppendLine();
            sb.AppendLine();
        }

        public static StringBuilder ToRegisterStringBuilder(StringBuilder sb, List<AopCodeBuilder> builders, AssemblyMetaData mateData)
        {
            sb ??= new StringBuilder();
            sb.AppendLine("namespace Microsoft.Extensions.DependencyInjection");
            sb.AppendLine("{");
            sb.AppendLine($"\tinternal static class AopClassExtensions");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tpublic static IServiceCollection RegisterAopClass(this IServiceCollection services)");
            sb.AppendLine("\t\t{");

            foreach (var aopAttribute in mateData.AopAttributeMetaDataList)
            {
                sb.AppendLine($"\t\t\tservices.AddTransient<{aopAttribute.Key}>();");
            }

            sb.AppendLine();

            foreach (var builder in builders)
            {
                if (builder._metaData.InterfaceMetaData.Any())
                {
                    sb.AppendLine($"\t\t\tservices.AddScoped<{builder._metaData.InterfaceMetaData.First().Key}, {builder._metaData.NameSpace}.{builder.ClassName}>();");
                }
                else
                {
                    sb.AppendLine($"\t\t\tservices.AddScoped<{builder._metaData.NameSpace}.{builder._metaData.Name}, {builder._metaData.NameSpace}.{builder.ClassName}>();");
                }
            }

            sb.AppendLine("\t\t\treturn services;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb;
        }
        
        /// <summary>
        /// 构建扩展代码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="additionalTexts"></param>
        /// <param name="meta"></param>
        public static void BuildExtend(SourceProductionContext context, ImmutableArray<AdditionalText> additionalTexts, AssemblyMetaData meta, StringBuilder sb)
        {
            var extendMapModels = GetExtendMapModels(additionalTexts);
            extendMapModels.ForEach(item => item.AopMetaDataModel = meta);
            RenderExtend(context, meta, extendMapModels, sb);
        }

        private static List<ExtendTemplateModel> GetExtendMapModels(ImmutableArray<AdditionalText> additionalTexts)
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
                             case "class_filter_expression":
                                 model.class_filter_expression = split[1];
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

        private static void RenderExtend(SourceProductionContext context, AssemblyMetaData meta, List<ExtendTemplateModel> extendMapModels, StringBuilder sb)
        {
            //TestEx();
            //var index = Class1.Test();
            //context.AddSource("testcombi", "//" + index.ToString());

            var index = Class1.Test2(meta);
            context.AddSource("testcombi", "// count " + index.ToString());

            return;

            foreach (var extendMapModel in extendMapModels)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;

                switch (extendMapModel.Type)
                {
                    case ExtendTemplateType.ClassTarget:
                        //RenderExtendByClassMetaData(context, meta, extendMapModel);
                        break;
                    case ExtendTemplateType.InterfaceTarget:
                        //RenderExtendByInterfaceMetaData(context, meta, extendMapModel);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        //private static void TestEx()
        //{
        //    var ruleParameters = new RuleParameter[] {
        //        new RuleParameter("RequestType","Sales"),
        //        new RuleParameter("RequestStatus", "Active"),
        //        new RuleParameter("RegistrationStatus", "InProcess")
        //    };

        //    var mainRule = new Rule
        //    {
        //        RuleName = "rule1",
        //        Operator = "And",
        //        Rules = new List<Rule>()
        //    };

        //    var dummyRule = new Rule
        //    {
        //        RuleName = "testRule1",
        //        RuleExpressionType = RuleExpressionType.LambdaExpression,
        //        Expression = "RequestType == \"vod\""
        //    };

        //    mainRule.Rules = mainRule.Rules.Append(dummyRule);
        //    var parser = new RuleExpressionParser(new ReSettings());
        //    var ruleDelegate = parser.Compile(dummyRule.Expression, ruleParameters);



        }

        //public void TestScriban(SourceProductionContext context)
        //{
        //    //var template = Scriban.Template.Parse("Hello {{name}}!");
        //    //var result = template.Render(new { Name = "World" }); // => "Hello World!"


        //    var template = Scriban.Template.Parse(@"
        //<ul id='products'>
        //  {{ for product in products }}
        //    <li>
        //      <h2>{{ product.name }}</h2>
        //           Price: {{ product.price }}
        //           {{ product.description | string.truncate 15 }}
        //    </li>
        //  {{ end }}
        //</ul>
        //");

        //    var ProductList = new List<dynamic>()
        //            {
        //                new {
        //                    name="name1",
        //                    price=123,
        //                    description="this is ProductList"
        //                },
        //                new {
        //                    name="name2",
        //                    price=98,
        //                    description="this is ProductList2"
        //                }
        //            };

        //    var result = template.Render(new { Products = ProductList });



        //    context.AddSource("TestScriban", DateTime.Now.ToString() + Environment.NewLine + result);




        //}

        //private static void RenderExtendByClassMetaData(SourceProductionContext context, AssemblyMetaData meta, ExtendTemplateModel extendMapModel)
        //{
        //    foreach (var classMetaData in meta.ClassMetaDataList)
        //    {
        //        if (context.CancellationToken.IsCancellationRequested)
        //            return;

        //        extendMapModel.ClassMetaData = classMetaData;

        //        //TemplateRender.RenderExtend(extendMapModel, d =>
        //        //{
        //        //    d.AddAssemblyReference(typeof(System.Collections.IList));
        //        //    d.AddAssemblyReference(typeof(System.Linq.Enumerable));
        //        //    d.AddAssemblyReference(typeof(System.Linq.IQueryable));
        //        //    d.AddAssemblyReference(typeof(System.Collections.Generic.IEnumerable<>));
        //        //    d.AddAssemblyReference(typeof(System.Collections.Generic.List<>));
        //        //    d.AddAssemblyReference(typeof(System.Diagnostics.Debug));
        //        //    d.AddAssemblyReference(typeof(System.Diagnostics.CodeAnalysis.SuppressMessageAttribute));
        //        //    d.AddAssemblyReference(typeof(System.Runtime.Versioning.ComponentGuaranteesAttribute));
        //        //    d.AddAssemblyReference(typeof(System.Linq.Expressions.BinaryExpression));

        //        //    d.AddAssemblyReference(_currentAssembly);
        //        //}, d =>
        //        //{
        //        //    d.Model = extendMapModel;
        //        //});

        //        foreach (var kv in extendMapModel.TemplateResult)
        //        {
        //            context.AddSource($"{kv.Key.Replace(".txt", "")}.cs", kv.Value);
        //        }

        //        extendMapModel.TemplateResult.Clear();
        //    }
        //}

        //private static void RenderExtendByClassMetaData(SourceProductionContext context, AssemblyMetaData meta, ExtendTemplateModel extendMapModel)
        //{
        //    foreach (var classMetaData in meta.ClassMetaDataList)
        //    {
        //        if (context.CancellationToken.IsCancellationRequested)
        //            return;

        //        extendMapModel.ClassMetaData = classMetaData;

        //        //TemplateRender.RenderExtend(extendMapModel, d =>
        //        //{
        //        //    d.AddAssemblyReference(typeof(System.Collections.IList));
        //        //    d.AddAssemblyReference(typeof(System.Linq.Enumerable));
        //        //    d.AddAssemblyReference(typeof(System.Linq.IQueryable));
        //        //    d.AddAssemblyReference(typeof(System.Collections.Generic.IEnumerable<>));
        //        //    d.AddAssemblyReference(typeof(System.Collections.Generic.List<>));
        //        //    d.AddAssemblyReference(typeof(System.Diagnostics.Debug));
        //        //    d.AddAssemblyReference(typeof(System.Diagnostics.CodeAnalysis.SuppressMessageAttribute));
        //        //    d.AddAssemblyReference(typeof(System.Runtime.Versioning.ComponentGuaranteesAttribute));
        //        //    d.AddAssemblyReference(typeof(System.Linq.Expressions.BinaryExpression));

        //        //    d.AddAssemblyReference(_currentAssembly);
        //        //}, d =>
        //        //{
        //        //    d.Model = extendMapModel;
        //        //});

        //        foreach (var kv in extendMapModel.TemplateResult)
        //        {
        //            context.AddSource($"{kv.Key.Replace(".txt", "")}.cs", kv.Value);
        //        }

        //        extendMapModel.TemplateResult.Clear();
        //    }
        //}

        //private void RenderExtendByInterfaceMetaData(SourceProductionContext context, AssemblyMetaData meta, ExtendTemplateModel extendMapModel)
        //{
        //    foreach (var interfaceMetaData in meta.InterfaceMetaDataList)
        //    {
        //        if (context.CancellationToken.IsCancellationRequested)
        //            return;

        //        extendMapModel.InterfaceMetaData = interfaceMetaData;

        //        TemplateRender.RenderExtend(extendMapModel, d =>
        //        {
        //            d.AddAssemblyReference(typeof(System.Collections.IList));
        //            d.AddAssemblyReference(typeof(Enumerable));
        //            d.AddAssemblyReference(_currentAssembly);
        //        }, d =>
        //        {
        //            d.Model = extendMapModel;
        //        });

        //        foreach (var kv in extendMapModel.TemplateResult)
        //        {
        //            context.AddSource($"{kv.Key.Replace(".txt", "")}.cs", kv.Value);
        //        }

        //        extendMapModel.TemplateResult.Clear();
        //    }
        //}

        
        
    //}
}