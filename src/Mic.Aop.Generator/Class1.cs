using Mic.Aop.Generator.MetaData;
using Mic.Aop.Generator.Renders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mic.Aop.Generator
{
    internal static class Class1
    {
        public static int Test()
        {
            var sourceCodeText = @"public class Ne98b821b577b4b8b8d8907a6151f1c48{
public static System.Int32 Invoke(System.String arg){return arg.Length;}}";


            var systemReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            //var annotationReference = MetadataReference.CreateFromFile(typeof(TableAttribute).Assembly.Location);
            //var weihanliCommonReference = MetadataReference.CreateFromFile(typeof(IDependencyResolver).Assembly.Location);

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCodeText, new CSharpParseOptions(LanguageVersion.Latest)); // 获取代码分析得到的语法树

            var assemblyName = $"DbTool.DynamicGenerated.aaa";

            // 创建编译任务
            var compilation = CSharpCompilation.Create(assemblyName) //指定程序集名称
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))//输出为 dll 程序集
                    .AddReferences(systemReference) //添加程序集引用
                    .AddSyntaxTrees(syntaxTree) // 添加上面代码分析得到的语法树
                ;
            //var assemblyPath = ApplicationHelper.MapPath($"{assemblyName}.dll");
            var stream = new MemoryStream();
            var compilationResult = compilation.Emit(stream); // 执行编译任务，并输出编译后的程序集
            if (compilationResult.Success)
            {
                var ass = Assembly.Load(stream.GetBuffer());
                var ins = ass.CreateInstance("Ne98b821b577b4b8b8d8907a6151f1c48");

                var type = ass.GetType("Ne98b821b577b4b8b8d8907a6151f1c48");
                if (type != null)
                {
                    var method = type.GetMethod("Invoke");
                    var obj = Activator.CreateInstance(type);
                    var result = method.Invoke(obj, new object?[] { "assdsfdf" });
                    return (int)result;
                }

            }
            else
            {

            }

            return 0;
        }

        public static string Test2(Mic.Aop.Generator.MetaData.AssemblyMetaData arg)
        {
            var corePath =
                "C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\8.0.0-alpha.1.22605.1\\System.Private.CoreLib.dll";

            var sourceCodeText = @"public class Ne98b821b577b4b8b8d8907a6151f1c48{
public static string Invoke(Mic.Aop.Generator.Renders.AssemblyMetaData2 arg){return arg.ClassMetaDataList.Count.ToString();}}";
            
            var systemReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            //systemReference = MetadataReference.CreateFromFile(corePath);
            //var annotationReference = MetadataReference.CreateFromFile(typeof(AssemblyMetaData).Assembly.Location);
            //var weihanliCommonReference = MetadataReference.CreateFromFile(typeof(IDependencyResolver).Assembly.Location);

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCodeText, new CSharpParseOptions(LanguageVersion.CSharp9)); // 获取代码分析得到的语法树

            var assemblyName = $"DbTool.DynamicGenerated.aaa2";

            // 创建编译任务
            var compilation = CSharpCompilation.Create(assemblyName) //指定程序集名称

                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))//输出为 dll 程序集
                    .AddReferences(systemReference) //添加程序集引用
                   

                    .AddSyntaxTrees(syntaxTree) // 添加上面代码分析得到的语法树
                ;






            var basePath = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
            var rumtime = basePath + "\\System.Runtime.dll";


            var rumre = MetadataReference.CreateFromFile(rumtime);
            compilation = compilation.AddReferences(rumre);
            //var runtimeAss = Assembly.Load(rumtime);

            var ss = typeof(AssemblyMetaData).Assembly.GetReferencedAssemblies();
            foreach (var name in ss)
            {
                var assembly = Assembly.Load(name);
                compilation = compilation.AddReferences(MetadataReference.CreateFromFile(assembly.Location));
            }

            compilation = compilation.AddReferences(MetadataReference.CreateFromFile(IncrementalGenerator.CurrentAssembly.Location));

            //compilation = compilation.AddReferences(MetadataReference.CreateFromFile(assembly.Location));

            //var path = "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\mscorlib.dll";
            //var as1 = Assembly.LoadFile(path);
            //if (as1 != null)
            //{
            //    compilation.AddReferences(MetadataReference.CreateFromFile(as1.Location));
            //}



            //

            //var assemblyPath = ApplicationHelper.MapPath($"{assemblyName}.dll");
            var stream = new MemoryStream();
            var compilationResult = compilation.Emit(stream); // 执行编译任务，并输出编译后的程序集
            if (compilationResult.Success)
            {
                var ass = Assembly.Load(stream.GetBuffer());
                var ins = ass.CreateInstance("Ne98b821b577b4b8b8d8907a6151f1c48");

                var type = ass.GetType("Ne98b821b577b4b8b8d8907a6151f1c48");
                if (type != null)
                {
                    var parm = new AssemblyMetaData2 { ClassMetaDataList = arg.ClassMetaDataList.Select(d => d.Name).ToList() };
                    //parm.ClassMetaDataList.Count();


                    var method = type.GetMethod("Invoke");
                    var obj = Activator.CreateInstance(type);
                    var result = method.Invoke(obj, new object[] { parm });
                    return result.ToString();
                }

            }
            else
            {
                
                return compilationResult.Diagnostics.FirstOrDefault()?.ToString();
            }

            return "--";
        }

        //public void TestCombin()
        //{
        //    var sourceCodeText =
        //        $"{usingList.StringJoin(Environment.NewLine)}{Environment.NewLine}{sourceCodeTextBuilder}"; // 获取完整的代码

        //    var systemReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        //    var annotationReference = MetadataReference.CreateFromFile(typeof(TableAttribute).Assembly.Location);
        //    var weihanliCommonReference = MetadataReference.CreateFromFile(typeof(IDependencyResolver).Assembly.Location);

        //    var syntaxTree = CSharpSyntaxTree.ParseText(sourceCodeText, new CSharpParseOptions(LanguageVersion.Latest)); // 获取代码分析得到的语法树

        //    var assemblyName = $"DbTool.DynamicGenerated.aaa";

        //    // 创建编译任务
        //    var compilation = CSharpCompilation.Create(assemblyName) //指定程序集名称
        //            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))//输出为 dll 程序集
        //            .AddReferences(systemReference, annotationReference, weihanliCommonReference) //添加程序集引用
        //            .AddSyntaxTrees(syntaxTree) // 添加上面代码分析得到的语法树
        //        ;
        //    var assemblyPath = ApplicationHelper.MapPath($"{assemblyName}.dll");
        //    var compilationResult = compilation.Emit(assemblyPath); // 执行编译任务，并输出编译后的程序集


        //    compilation.em

        //    if (compilationResult.Success)
        //    {
        //        // 编译成功，获取编译后的程序集并从中获取数据库表信息以及字段信息
        //        try
        //        {
        //            byte[] assemblyBytes;
        //            using (var fs = File.OpenRead(assemblyPath))
        //            {
        //                assemblyBytes = fs.ToByteArray();
        //            }
        //            return GeTableEntityFromAssembly(Assembly.Load(assemblyBytes));
        //        }
        //        finally
        //        {
        //            File.Delete(assemblyPath); // 清理资源
        //        }
        //    }
        //}
    }
}
