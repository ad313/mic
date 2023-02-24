using Mic.Aop.Generator.MetaData;
using Microsoft.CodeAnalysis;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mic.Aop.Generator.Renders
{
    internal partial class TemplateRender
    {
        /// <summary>
        /// 构建扩展代码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="additionalTexts"></param>
        /// <param name="meta"></param>
        public static void BuildExtend(SourceProductionContext context, ImmutableArray<AdditionalText> additionalTexts, AssemblyMetaData meta, StringBuilder sb)
        {
            var extendMapModels = GetExtendMapModels(additionalTexts);
            //extendMapModels.ForEach(item => item.AopMetaDataModel = meta);
            RenderExtend(context, meta, extendMapModels, sb);
        }

        private static readonly ConcurrentDictionary<string, Template> TemplateDictionary = new ConcurrentDictionary<string, Template>();

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
                         var key = split[0]?.Trim();
                         var value = split[1]?.Trim();
                         switch (key?.ToLower())
                         {
                             case "type":
                                 model.Type = (ExtendTemplateType)(int.TryParse(value, out int v) ? v : 0);
                                 break;
                             case "name":
                                 model.Name = value;
                                 break;
                             case "code":
                                 model.Code = value;
                                 break;
                             case "templates":
                                 model.Templates = value.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => d.Trim()).ToList();
                                 break;
                             case "class_attribute_filter_expression":
                                 model.class_attribute_filter_expression = value;
                                 break;
                             case "class_property_attribute_filter_expression":
                                 model.class_property_attribute_filter_expression = value;
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
                        //RenderExtendByInterfaceMetaData(context, meta, extendMapModel);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void RenderExtendByClassMetaData(SourceProductionContext context, AssemblyMetaData meta, ExtendTemplateModel extendMapModel)
        {
            var classData = meta.ClassMetaDataList.Select(d => new TemplateClassMetaData(d.Namespace, d.Name,
                d.AttributeMetaData, d.PropertyMeta, d.MethodMetaData, d.Interfaces, d.Constructor, d.Usings, d.AccessModifier)).ToList();

            if (!string.IsNullOrWhiteSpace(extendMapModel.class_attribute_filter_expression))
            {
                classData = classData.Where(d => d.HasAttribute(extendMapModel.class_attribute_filter_expression)).ToList();
            }

            foreach (var classMetaData in classData)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;

                foreach (var kv in extendMapModel.TemplateDictionary)
                {
                    var scriptObject1 = new FilterFunctions();
                    scriptObject1.Import(classMetaData);
                    var scContext = new TemplateContext();
                    scContext.PushGlobal(scriptObject1);

                    var template = TemplateDictionary.GetOrAdd(kv.Key, key => Template.Parse(kv.Value));
                    var result = template.Render(scContext);
                    extendMapModel.TemplateResult.TryAdd($"{kv.Key.Replace(".txt", "")}_{classMetaData.Name}_g.cs", result);
                }

                foreach (var keyValuePair in extendMapModel.TemplateResult)
                {
                    context.AddSource(keyValuePair.Key, keyValuePair.Value);
                }

                extendMapModel.TemplateResult.Clear();
            }
        }



        //        public static void TestScriban(SourceProductionContext context, AssemblyMetaData meta, ExtendTemplateModel extendMapModel)
        //        {
        //            var tem = @"
        //{{- for using in Usings }}
        //{{- using}}
        //{{ end }}
        //namespace {{ Namespace }}
        //{
        //    {{ SplitString AccessModifier 0 }} partial class {{ Name }}
        //    {
        //    {{- for prop in (PropertyListAttributeFilter PropertyMeta 'BizDictionaryAttribute') }}
        //        /// <summary>
        //        /// {{ prop.Description }}
        //        /// </summary>
        //        {{ prop.AccessModifier}} string {{prop.Name}}Text { get; set; }
        //    {{ end -}}
        //    }
        //}";
        //            //Debugger.Launch();

        //            //{{if PropertyHasAttribute prop 'BizDictionaryAttribute'}}{{prop.AccessModifier}} string {{prop.Name}}Text { get; set; }


        //            var scriptObject1 = new FilterFunctions();
        //            var scContext = new TemplateContext();
        //            scContext.PushGlobal(scriptObject1);

        //            var template = Scriban.Template.Parse(tem);

        //            var classData = meta.ClassMetaDataList.Select(d => new TemplateClassMetaData(d.Namespace, d.Name,
        //                d.AttributeMetaData, d.PropertyMeta, d.MethodMetaData, d.Interfaces, d.Constructor, d.Usings, d.AccessModifier)).ToList();

        //            if (!string.IsNullOrWhiteSpace(extendMapModel.class_attribute_filter_expression))
        //            {
        //                classData = classData.Where(d => d.HasAttribute(extendMapModel.class_attribute_filter_expression)).ToList();
        //            }

        //            foreach (var data in classData)
        //            {
        //                scriptObject1.Import(data);

        //                var result = template.Render(scContext);
        //                context.AddSource($"TestScriban_{data.Name}", result);
        //            }
        //        }
    }

   

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