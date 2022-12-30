//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Mic.Aop.Generator.MetaData;
//using Microsoft.CodeAnalysis.Text;

//namespace Mic.Aop.Generator
//{
//    /// <summary>
//    /// BizDictionary 代码构建器
//    /// </summary>
//    sealed class AopCodeBuilder : IEquatable<AopCodeBuilder>
//    {
//        /// <summary>
//        /// 接口符号
//        /// </summary>
//        public readonly AopCodeBuilderMetaData _metaData;

//        /// <summary>
//        /// using
//        /// </summary>
//        public IEnumerable<string> Using
//        {
//            get
//            {
//                yield return "using Microsoft.Extensions.DependencyInjection;";
//                yield return "using System.Diagnostics;";
//                yield return "using System.Threading.Tasks;";
//                yield return "using System.Collections.Generic;";
//                yield return "using System.Linq;";
//                yield return "using Mic.Aop;";
//            }
//        }

//        /// <summary>
//        /// 命名空间
//        /// </summary>
//        public string Namespace => $"{this._metaData.NameSpace}";

//        /// <summary>
//        /// 文件名称
//        /// </summary>
//        public string SourceCodeName => $"{this.Namespace.Split('.').LastOrDefault()}_{this.ClassName}.cs";
        
//        /// <summary>
//        /// 类名
//        /// </summary>
//        public string ClassName => this._metaData.Name+"_Aop";
        
//        /// <summary>
//        /// 代码构建器
//        /// </summary>
//        /// <param name="metaData"></param>
//        public AopCodeBuilder(AopCodeBuilderMetaData metaData)
//        {
//            this._metaData = metaData;
//        }

//        /// <summary>
//        /// 转换为SourceText
//        /// </summary>
//        /// <returns></returns>
//        public SourceText ToSourceText()
//        {
//            var code = this.ToString();
//            return SourceText.From(code, Encoding.UTF8);
//        }

//        public StringBuilder ToTraceStringBuilder(StringBuilder? builder)
//        {
//            builder ??= new StringBuilder();

//            builder.AppendLine($"// -------------------------------------------------");
//            builder.AppendLine($"//命名空间：{Namespace}");
//            builder.AppendLine($"//文件名称：{SourceCodeName}");
//            builder.AppendLine($"//接口：{string.Join("、", _metaData.InterfaceMetaData.Select(d => d.Key))}");
//            builder.AppendLine($"//类名：{ClassName}");

//            foreach (var methodData in _metaData.MethodMetaDatas)
//            {
//                builder.AppendLine();
//                builder.AppendLine($"//方法名称：{methodData.Name}");
//                builder.AppendLine($"//Key名称：{methodData.Key}");
//                builder.AppendLine($"//Aop属性：{string.Join("、", methodData.AttributeMetaData.Select(d => d.Name))}");
//            }

//            builder.AppendLine();
//            builder.AppendLine();

//            return builder;
//        }

//        /// <summary>
//        /// 转换为字符串
//        /// </summary>
//        /// <returns></returns>
//        public override string ToString()
//        {
//            foreach (var item in this.Using)
//            {
//                if (!_metaData.Usings.Contains(item))
//                    _metaData.Usings.Add(item);
//            }

//            var builder = new StringBuilder();
//            foreach (var item in _metaData.Usings.Distinct().Select(d => d.Split(' ').Last()).OrderBy(d => d))
//            {
//                builder.AppendLine($"using {item}");
//            }

//            builder.AppendLine();

//            builder.AppendLine($"namespace {this.Namespace}");
//            builder.AppendLine("{");
//            builder.AppendLine($"\tpublic sealed class {ClassName} : {this._metaData.Name}");
//            builder.AppendLine("\t{");

//            BuildConstructor(builder);

//            foreach (var propertyMeta in _metaData.MethodMetaDatas)
//            {
//                propertyMeta.BuildCode(builder);
//            }
            
//            builder.AppendLine("\t}");
//            builder.AppendLine("}");

//            return builder.ToString();
//        }

//        private void BuildConstructor(StringBuilder builder)
//        {
//            builder.AppendLine($"\t\tprivate readonly IServiceProvider _serviceProvider0;");

//            if (_metaData.Constructor.Any())
//            {
//                builder.AppendLine($"\t\tpublic {ClassName}(IServiceProvider serviceProvider0, {string.Join(", ", _metaData.Constructor.Select(d => $"{d.Key} {d.Value}"))}) : base({string.Join(", ", _metaData.Constructor.Select(d => d.Value))})");
//            }
//            else
//            {
//                builder.AppendLine($"\t\tpublic {ClassName}(IServiceProvider serviceProvider0)");
//            }

//            builder.AppendLine("\t\t{");
//            builder.AppendLine($"\t\t\t_serviceProvider0 = serviceProvider0;");
//            builder.AppendLine("\t\t}");
//            builder.AppendLine();
//        }

//        /// <summary>
//        /// 是否与目标相等
//        /// </summary>
//        /// <param name="other"></param>
//        /// <returns></returns>
//        public bool Equals(AopCodeBuilder other)
//        {
//            return $"{this.Namespace}_{this.ClassName}" == $"{other.Namespace}_{other.ClassName}";
//        }

//        /// <summary>
//        /// 是否与目标相等
//        /// </summary>
//        /// <param name="obj"></param>
//        /// <returns></returns>
//        public override bool Equals(object obj)
//        {
//            if (obj is AopCodeBuilder builder)
//            {
//                return this.Equals(builder);
//            }
//            return false;
//        }

//        /// <summary>
//        /// 获取哈希
//        /// </summary>
//        /// <returns></returns>
//        public override int GetHashCode()
//        {
//            return this.SourceCodeName.GetHashCode();
//        }
//    }
//}