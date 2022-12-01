using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mic.Aop.Generator.MetaData
{
    /// <summary>
    /// 方法元数据
    /// </summary>
    public class MethodMetaData : MetaDataBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="attributeMetaData">attribute</param>
        /// <param name="returnValue">返回值</param>
        /// <param name="param">输入参数</param>
        /// <param name="perfix">方法修饰符</param>
        public MethodMetaData(string name, List<AttributeMetaData> attributeMetaData, string returnValue, List<KeyValueModel> param, string perfix) : base(name, attributeMetaData)
        {
            ReturnValue = returnValue;
            Param = param;

            HasReturnValue = !string.IsNullOrWhiteSpace(returnValue) && returnValue != "void" && returnValue != "Task";
            IsTask = returnValue?.StartsWith("Task") == true;

            CanOverride = !string.IsNullOrWhiteSpace(perfix) && perfix.Contains(" ") && new List<string>() { "virtual", "override" }.Contains(perfix
                .Replace("public", "").Trim()
                .Split(' ').First());

            Key = GetKey(name, param);
        }

        /// <summary>
        /// 方法唯一标识符，区分方法重载
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public string ReturnValue { get; private set; }
        /// <summary>
        /// 是否有返回值
        /// </summary>
        public bool HasReturnValue { get; private set; }
        /// <summary>
        /// 是否是异步
        /// </summary>
        public bool IsTask { get; private set; }
        /// <summary>
        /// 能否被重写
        /// </summary>
        public bool CanOverride { get; private set; }
        /// <summary>
        /// 输入参数
        /// </summary>
        public List<KeyValueModel> Param { get; private set; }

        public void BuildCode(StringBuilder builder)
        {
            var constructor = string.Join(", ", Param.Select(d => $"{d.Key} {d.Value}"));
            builder.AppendLine($"\t\tpublic override{(IsTask ? " async" : null)} {ReturnValue} {Name}({constructor})");
            builder.AppendLine("\t\t{");

            //AopContext
            builder.AppendLine($"\t\t\tvar aopContext = new AopContext(_serviceProvider0,");
            var dicParam = string.Join(", ", Param.Select(d => "{ " + $"\"{d.Value}\", {d.Value}" + " }"));
            builder.AppendLine("\t\t\t\tnew Dictionary<string, dynamic>() { " + dicParam + " },");
            
            builder.AppendLine(IsTask ? "\t\t\t\ttrue," : "\t\t\t\tfalse,");
            builder.AppendLine(HasReturnValue ? "\t\t\t\ttrue," : "\t\t\t\tfalse,");
            builder.AppendLine("\t\t\t\tnull);");
            builder.AppendLine();
            
            //AopInterceptor
            for (var i = 0; i < AttributeMetaData.Count; i++)
            {
                //builder.AppendLine($"\t\t\tvar aopInterceptor{i} = new {AttributeMetaData[i].Name}Attribute()");
                builder.AppendLine($"\t\t\tvar aopInterceptor{i} = _serviceProvider0.GetRequiredService<{AttributeMetaData[i].Name}Attribute>();");
                foreach (var pair in AttributeMetaData[i].ParamDictionary)
                {
                    builder.AppendLine($"\t\t\taopInterceptor{i}.{pair.Key} = {pair.Value};");
                }
            }

            for (var i = 0; i < AttributeMetaData.Count; i++)
            {
                builder.AppendLine(IsTask
                    ? $"\t\t\tif (aopInterceptor{i}.HasBefore) aopContext = await aopInterceptor{i}.BeforeAsync(aopContext);"
                    : $"\t\t\tif (aopInterceptor{i}.HasBefore) aopContext = aopInterceptor{i}.Before(aopContext);");
            }
            
            //给AopContext添加执行方法
            var baseConstructor = string.Join(", ", Param.Select(d => $"{d.Value}"));
            for (var i = AttributeMetaData.Count - 1; i >= 0; i--)
            {
                if (i == AttributeMetaData.Count - 1)
                {
                    builder.AppendLine($"\t\t\tif (aopInterceptor{i}.HasAopNext)");
                    builder.AppendLine("\t\t\t{");
                    builder.AppendLine($"\t\t\t\tif (aopInterceptor{i}.HasActualNext)");
                    builder.AppendLine("\t\t\t\t{");
                    
                    //没有返回值 且 是同步方法，通Task.Run 包装一下
                    if (!HasReturnValue && !IsTask)
                    {
                        builder.AppendLine($"\t\t\t\t\taopContext.ActualMethod = () => Task.Run(() => base.{Name}({baseConstructor}));");
                    }
                    else
                    {
                        builder.AppendLine($"\t\t\t\t\taopContext.ActualMethod = () => base.{Name}({baseConstructor});");
                    }
                    builder.AppendLine("\t\t\t\t}");
                    builder.AppendLine(IsTask
                        ? $"\t\t\t\taopContext = await aopInterceptor{i}.NextAsync(aopContext);"
                        : $"\t\t\t\taopContext = aopInterceptor{i}.Next(aopContext);");
                    builder.AppendLine("\t\t\t}");

                    builder.AppendLine("\t\t\telse");
                    builder.AppendLine("\t\t\t{");
                    builder.AppendLine($"\t\t\t\tif (aopInterceptor{i}.HasActualNext)");
                    builder.AppendLine("\t\t\t\t{");

                    var method = $"base.{Name}({baseConstructor})";
                    if (HasReturnValue)
                    {
                        if (IsTask)
                        {
                            builder.AppendLine($"\t\t\t\t\taopContext.ReturnValue = await {method};");
                        }
                        else
                        {
                            builder.AppendLine($"\t\t\t\t\taopContext.ReturnValue = {method};");
                        }
                    }
                    else
                    {
                        if (IsTask)
                        {
                            builder.AppendLine($"\t\t\t\t\tawait {method};");
                        }
                        else
                        {
                            builder.AppendLine($"\t\t\t\t\t{method};");
                        }
                    }
                    
                    builder.AppendLine("\t\t\t\t}");
                    builder.AppendLine("\t\t\t}");
                }

                builder.AppendLine(IsTask
                    ? $"\t\t\tif (aopInterceptor{i}.HasAfter) aopContext = await aopInterceptor{i}.AfterAsync(aopContext);"
                    : $"\t\t\tif (aopInterceptor{i}.HasAfter) aopContext = aopInterceptor{i}.After(aopContext);");
            }

            if (HasReturnValue)
            {
                builder.AppendLine();
                builder.AppendLine($"\t\t\treturn aopContext.ReturnValue;");
            }

            builder.AppendLine("\t\t}");

            builder.AppendLine();
        }
    }
}