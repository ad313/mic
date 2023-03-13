using Kooboo.Json;
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
            RenderExtendMain(context, meta, extendMapModels, sb);
        }

        private static List<ExtendTemplateModel> GetExtendMapModels(ImmutableArray<AdditionalText> additionalTexts)
        {
            var json = additionalTexts.FirstOrDefault(d => d.Path.EndsWith("Map.txt", StringComparison.OrdinalIgnoreCase))?.GetText()?.ToString();
            if (string.IsNullOrWhiteSpace(json)) 
                return new List<ExtendTemplateModel>();

            var list = JsonSerializer.ToObject<List<ExtendTemplateModel>>(json);
            foreach (var model in list)
            {
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

                if (!string.IsNullOrWhiteSpace(model.MainTemplate))
                {
                    model.MainTemplateString = additionalTexts.FirstOrDefault(d =>
                            d.Path.EndsWith($"\\{model.MainTemplate}", StringComparison.OrdinalIgnoreCase))?.GetText()
                        ?.ToString();
                }
            }

            //默认附加 枚举业务扩展
            var bizDicModel = list.FirstOrDefault(d => d.Code.Equals("BizEnumExtendBuilder", StringComparison.OrdinalIgnoreCase));
            if (bizDicModel == null)
            {
                bizDicModel = new ExtendTemplateModel()
                {
                    Code = "BizEnumExtendBuilder",
                    Name = "枚举业务扩展",
                    MainTemplate = "BizEnumExtend_Main.txt",
                    Templates = new List<string>() { "BizEnumExtend.txt" },
                };

                foreach (var template in bizDicModel.Templates)
                {
                    var file = Assembly.GetExecutingAssembly().GetResourceString($"Templates.{template}");
                    bizDicModel.TemplateDictionary ??= new ConcurrentDictionary<string, string>();
                    bizDicModel.TemplateDictionary.TryAdd(template, file);
                }

                bizDicModel.MainTemplateString = Assembly.GetExecutingAssembly().GetResourceString($"Templates.{bizDicModel.MainTemplate}");

                list.Add(bizDicModel);
            }

            return list;
        }

        private static void RenderExtendMain(SourceProductionContext context, AssemblyMetaData meta, List<ExtendTemplateModel> extendMapModels, StringBuilder sb)
        {
            foreach (var extendMapModel in extendMapModels)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;
                
                if (string.IsNullOrWhiteSpace(extendMapModel.MainTemplateString))
                    continue;

                var scriptObject1 = new FilterFunctions();
                scriptObject1.Import(new { meta_data = meta, template_data = extendMapModel });
                var scContext = new TemplateContext();
                scContext.PushGlobal(scriptObject1);

                var template = Template.Parse(extendMapModel.MainTemplateString);
                template.Render(scContext);

                extendMapModel.TemplateResult.Clear();
            }
        }
    }
}