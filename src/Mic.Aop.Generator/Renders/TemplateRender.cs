using Mic.Aop.Generator.Extend;
using Mic.Aop.Generator.MetaData;
using RazorEngineCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mic.Aop.Generator.Renders
{
    internal class TemplateRender
    {
        public static ConcurrentDictionary<string, IRazorEngineCompiledTemplate<AopTemplateModel>> TemplateCache { get; set; } =
            new ConcurrentDictionary<string, IRazorEngineCompiledTemplate<AopTemplateModel>>();

        public static ConcurrentDictionary<string, IRazorEngineCompiledTemplate<ExtendTemplateModel>> ExtendTemplateCache { get; set; } =
            new ConcurrentDictionary<string, IRazorEngineCompiledTemplate<ExtendTemplateModel>>();

        #region Render

        public static async Task<string> RenderAsync<T>(Assembly assembly,
            string name,
            Action<IRazorEngineCompilationOptionsBuilder> actionCompile,
            Action<T> actionInitData) where T : RazorEngineTemplateBase
        {
            var templateContent = assembly.GetResourceString(name);
            if (string.IsNullOrWhiteSpace(templateContent))
                throw new ArgumentNullException(nameof(templateContent));

            IRazorEngine razorEngine = new RazorEngine();
            var template = razorEngine.Compile<T>(templateContent, actionCompile);

            return await template.RunAsync(actionInitData);
        }

        public static string Render<T>(Assembly assembly,
            string name,
            Action<IRazorEngineCompilationOptionsBuilder> actionCompile,
            Action<T> actionInitData) where T : RazorEngineTemplateBase
        {
            var templateContent = assembly.GetResourceString(name);
            if (string.IsNullOrWhiteSpace(templateContent))
                throw new ArgumentNullException(nameof(templateContent));

            IRazorEngine razorEngine = new RazorEngine();
            var template = razorEngine.Compile<T>(templateContent, actionCompile);

            return template.Run(actionInitData);
        }

        public static string Render(Assembly assembly,
            string name,
            Action<IRazorEngineCompilationOptionsBuilder> actionCompile,
            Action<AopTemplateModel> actionInitData)
        {
            if (!TemplateCache.TryGetValue(name, out IRazorEngineCompiledTemplate<AopTemplateModel> template))
            {
                var templateContent = assembly.GetResourceString(name);
                if (string.IsNullOrWhiteSpace(templateContent))
                    throw new ArgumentNullException(nameof(templateContent));

                IRazorEngine razorEngine = new RazorEngine();
                template = razorEngine.Compile<AopTemplateModel>(templateContent, actionCompile);

                TemplateCache.TryAdd(name, template);
            }

            return template.Run(actionInitData);
        }

        public static void RenderExtend(ExtendTemplateModel mapModel,
            Action<IRazorEngineCompilationOptionsBuilder> actionCompile,
            Action<ExtendTemplateModel> actionInitData)
        {
            foreach (var keyValuePair in mapModel.TemplateDictionary)
            {
                var name = mapModel.GetName(keyValuePair.Key);
                if (!ExtendTemplateCache.TryGetValue(name, out IRazorEngineCompiledTemplate<ExtendTemplateModel> template))
                {
                    var templateContent = keyValuePair.Value;
                    if (string.IsNullOrWhiteSpace(templateContent))
                        throw new ArgumentNullException(nameof(templateContent));

                    IRazorEngine razorEngine = new RazorEngine();
                    template = razorEngine.Compile<ExtendTemplateModel>(templateContent, actionCompile);

                    ExtendTemplateCache.TryAdd(name, template);
                }

                var result = template.Run(actionInitData);
                result = result.Replace("\r\n", "").Trim();
                if (!string.IsNullOrWhiteSpace(result))
                    mapModel.TemplateResult.TryAdd($"{name}.{mapModel.ClassMetaData.Name}", result);
            }
        }

        #endregion

        public static StringBuilder ToError(Assembly assembly, Exception e)
        {
            var sb = new StringBuilder();
            var result = TemplateRender.Render(assembly, "Template.Error.cshtml", d =>
            {
                d.AddAssemblyReference(typeof(System.Collections.IList));
                d.AddAssemblyReference(typeof(Enumerable));
                d.AddAssemblyReference(assembly);
            },
                d =>
                {
                    d.ExceptionModel = e;
                });

            sb.Append(result);
            return sb;
        }

        public static StringBuilder ToRegisterCode(Assembly assembly, List<AopCodeBuilder> builders, AssemblyMetaData mateData)
        {
            var sb = new StringBuilder();
            var result = TemplateRender.Render(assembly, "Template.RegisterAopClass.cshtml", d =>
            {
                d.AddAssemblyReference(typeof(System.Collections.IList));
                d.AddAssemblyReference(typeof(Enumerable));
                d.AddAssemblyReference(assembly);
            },
                d =>
                {
                    d.MetaModel = mateData;
                    d.AopCodeBuilderModel = builders;
                });

            sb.Append(result);
            return sb;
        }

        public static StringBuilder ToTrace(Assembly assembly, List<AopCodeBuilder> builders, AssemblyMetaData mateData)
        {
            var builder = new StringBuilder();
            var result = TemplateRender.Render(assembly, "Template.Trace.cshtml", d =>
            {
                d.AddAssemblyReference(typeof(System.Collections.IList));
                d.AddAssemblyReference(typeof(Enumerable));
                d.AddAssemblyReference(assembly);
            },
                d =>
                {
                    d.MetaModel = mateData;
                    d.AopCodeBuilderModel = builders;
                });

            builder.Append(result);
            return builder;
        }
    }
}