using Mic.Aop.Generator.Extend;
using Mic.Aop.Generator.MetaData;
using Microsoft.CodeAnalysis;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
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
            RenderExtend(context, meta, extendMapModels, sb);
        }

        private static List<ExtendTemplateModel> GetExtendMapModels(ImmutableArray<AdditionalText> additionalTexts)
        {
            var mapText = additionalTexts.FirstOrDefault(d => d.Path.EndsWith("Map.txt", StringComparison.OrdinalIgnoreCase))?.GetText()?.ToString();
            mapText ??= "";
            var list = Regex.Split(mapText, "#")
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


            //默认附加 枚举业务扩展
            var bizDicModel = list.FirstOrDefault(d => d.Code.Equals("BizEnumExtendBuilder", StringComparison.OrdinalIgnoreCase));
            if (bizDicModel == null)
            {
                bizDicModel = new ExtendTemplateModel()
                {
                    Code = "BizEnumExtendBuilder",
                    Type = ExtendTemplateType.ClassTarget,
                    Name = "枚举业务扩展",
                    class_attribute_filter_expression = "BizDictionaryAttribute",
                    Templates = new List<string>() { "BizEnumExtend.txt" },
                };

                foreach (var template in bizDicModel.Templates)
                {
                    var file = Assembly.GetExecutingAssembly().GetResourceString($"Templates.{template}");
                    bizDicModel.TemplateDictionary ??= new ConcurrentDictionary<string, string>();
                    bizDicModel.TemplateDictionary.TryAdd(template, file);
                }

                list.Add(bizDicModel);
            }
            return list;
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
                        RenderExtendByInterfaceMetaData(context, meta, extendMapModel);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void RenderExtendByClassMetaData(SourceProductionContext context, AssemblyMetaData meta, ExtendTemplateModel extendMapModel)
        {
            var source = meta.ClassMetaDataList;
            if (!string.IsNullOrWhiteSpace(extendMapModel.class_attribute_filter_expression))
            {
                source = source.Where(d => d.AttributeMetaData.HasAttribute(extendMapModel.class_attribute_filter_expression)).ToList();
            }

            foreach (var item in source)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;

                foreach (var kv in extendMapModel.TemplateDictionary)
                {
                    var scriptObject1 = new FilterFunctions();
                    scriptObject1.Import(item);
                    var scContext = new TemplateContext();
                    scContext.PushGlobal(scriptObject1);

                    var template = Template.Parse(kv.Value);
                    var result = template.Render(scContext);
                    extendMapModel.TemplateResult.TryAdd($"{kv.Key.Replace(".txt", "")}_{item.Name}_g.cs", result);
                }

                foreach (var keyValuePair in extendMapModel.TemplateResult)
                {
                    context.AddSource(keyValuePair.Key, keyValuePair.Value);
                }

                extendMapModel.TemplateResult.Clear();
            }
        }

        private static void RenderExtendByInterfaceMetaData(SourceProductionContext context, AssemblyMetaData meta, ExtendTemplateModel extendMapModel)
        {
            var source = meta.InterfaceMetaDataList;
            if (!string.IsNullOrWhiteSpace(extendMapModel.class_attribute_filter_expression))
            {
                source = source.Where(d => d.AttributeMetaData.HasAttribute(extendMapModel.class_attribute_filter_expression)).ToList();
            }

            foreach (var item in source)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;

                foreach (var kv in extendMapModel.TemplateDictionary)
                {
                    var scriptObject1 = new FilterFunctions();
                    scriptObject1.Import(item);
                    var scContext = new TemplateContext();
                    scContext.PushGlobal(scriptObject1);

                    var template = Template.Parse(kv.Value);
                    var result = template.Render(scContext);
                    extendMapModel.TemplateResult.TryAdd($"{kv.Key.Replace(".txt", "")}_{item.Name}_g.cs", result);
                }

                foreach (var keyValuePair in extendMapModel.TemplateResult)
                {
                    context.AddSource(keyValuePair.Key, keyValuePair.Value);
                }

                extendMapModel.TemplateResult.Clear();
            }
        }
    }
}