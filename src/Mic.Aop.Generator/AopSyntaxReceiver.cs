using Mic.Aop.Generator.Extend;
using Mic.Aop.Generator.MetaData;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mic.Aop.Generator
{
    /// <summary>
    /// Aop 语法接收器
    /// </summary>
    sealed class AopSyntaxReceiver : ISyntaxReceiver
    {
        private const string GeneratorTagName = "AopInterceptor";
        private const string IgnoreAttribute = "IgnoreAopAttribute";
        /// <summary>
        /// 类列表
        /// </summary>
        private readonly List<ClassDeclarationSyntax> _classSyntaxList;
        /// <summary>
        /// 接口列表
        /// </summary>
        private readonly List<InterfaceDeclarationSyntax> _interfaceSyntaxList;
        /// <summary>
        /// 所有的AopInterceptor
        /// </summary>
        public List<string> AopAttributeList = new List<string>();
        /// <summary>
        /// 所有的AopInterceptor
        /// </summary>
        public List<ClassMetaData> AopAttributeMetaDataList = new List<ClassMetaData>();

        public AopSyntaxReceiver(List<ClassDeclarationSyntax> classSyntaxList, List<InterfaceDeclarationSyntax> interfaceSyntaxList)
        {
            _classSyntaxList = classSyntaxList;
            _interfaceSyntaxList = interfaceSyntaxList;
        }

        /// <summary>
        /// 访问语法树 
        /// </summary>
        /// <param name="syntaxNode"></param>
        void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InterfaceDeclarationSyntax interfaceSyntax)
            {
                this._interfaceSyntaxList.Add(interfaceSyntax);
            }

            if (syntaxNode is ClassDeclarationSyntax classSyntax)
            {
                this._classSyntaxList.Add(classSyntax);
            }
        }

        /// <summary>
        /// 找出所有 AopInterceptor class
        /// </summary>
        /// <returns></returns>
        public AopSyntaxReceiver FindAopInterceptors()
        {
            foreach (var classSyntax in this._classSyntaxList)
            {
                var root = classSyntax.SyntaxTree.GetRoot();
                var classesWithAttribute = root
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .ToList();

                if (!classesWithAttribute.Any())
                    continue;

                foreach (var classDeclarationSyntax in classesWithAttribute)
                {
                    if (classDeclarationSyntax.BaseList == null)
                        continue;

                    foreach (BaseTypeSyntax baseTypeSyntax in classDeclarationSyntax.BaseList.Types)
                    {
                        if (baseTypeSyntax.ToString().Trim() == GeneratorTagName)
                        {
                            AopAttributeList.Add(classDeclarationSyntax.Identifier.Text);

                            var meta = GetClassMetaData(classSyntax);
                            if (meta != null && AopAttributeMetaDataList.All(d => d.Name != meta.Name))
                                AopAttributeMetaDataList.Add(meta);
                        }
                    }
                }
            }

            AopAttributeList = AopAttributeList.Distinct().ToList();

            return this;
        }

        /// <summary>
        /// 获取所有接口和类
        /// </summary>
        /// <param name="compilation"></param>
        /// <returns></returns>
        public AssemblyMetaData GetMetaData(Compilation compilation)
        {
            var result = new AssemblyMetaData(AopAttributeList, IgnoreAttribute, new List<InterfaceMetaData>(), new List<ClassMetaData>());
            
            //处理接口
            foreach (var classSyntax in this._interfaceSyntaxList)
            {
                var root = classSyntax.SyntaxTree.GetRoot();
                var interfaceWithAttributeList = root
                    .DescendantNodes()
                    .OfType<InterfaceDeclarationSyntax>()
                    .ToList();

                if (!interfaceWithAttributeList.Any())
                    continue;

                foreach (var interfaceDeclaration in interfaceWithAttributeList)
                {
                    var namespaceName = interfaceDeclaration.FindParent<NamespaceDeclarationSyntax>().Name.ToString();
                    var className = interfaceDeclaration.Identifier.Text;
                    var properties = interfaceDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
                    var methodSyntax = interfaceDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

                    //属性集合
                    var props = properties.Select(d => new PropertyMetaData(d.Identifier.Text, d.GetAttributeMetaData(), GetPropertyDescription(d), d.Modifiers.ToString(), d.Type?.ToString())).ToList();
                    //方法集合
                    var methods = methodSyntax.Select(GetMethodMetaData).ToList();

                    var interfaceMetaData = new InterfaceMetaData(namespaceName, className, interfaceDeclaration.GetAttributeMetaData(), props, methods, interfaceDeclaration.Modifiers.ToString(), null);
                    if (interfaceMetaData.MethodMetaData.Any() && !result.InterfaceMetaDataList.Exists(d => d.Equals(interfaceMetaData)))
                        result.InterfaceMetaDataList.Add(interfaceMetaData);
                }
            }

            //处理类
            foreach (var classSyntax in this._classSyntaxList)
            {
                var root = classSyntax.SyntaxTree.GetRoot();
                var classesWithAttributeList = root
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .ToList();

                if (!classesWithAttributeList.Any())
                    continue;

                foreach (var classDeclaration in classesWithAttributeList)
                {
                    var classMetaData = GetClassMetaData(classDeclaration);
                    if (classMetaData == null)
                        continue;
                    
                    if (AopAttributeList.Contains(classMetaData.Name))
                        continue;

                    if (!result.ClassMetaDataList.Exists(d => d.Equals(classMetaData)))
                    {
                        //实现的接口
                        classMetaData.Usings.Add(classMetaData.Namespace);
                        classMetaData.InterfaceMetaData = result.InterfaceMetaDataList.Where(d => classMetaData.Interfaces.Contains(d.Key)
                            || classMetaData.Interfaces.SelectMany(t => classMetaData.Usings.Select(u => $"{u.Replace("using ", "").Replace(";", "")}.{t.Split('.').Last()}")).Contains(d.Key)).ToList();
                        classMetaData.Usings.Remove(classMetaData.Namespace);
                        
                        result.ClassMetaDataList.Add(classMetaData);
                    }
                }
            }

            result.AopAttributeMetaDataList = AopAttributeMetaDataList;

            return result;
        }

        private ClassMetaData GetClassMetaData(ClassDeclarationSyntax classDeclaration)
        {
            try
            {
                var namespaceName = classDeclaration.FindParent<NamespaceDeclarationSyntax>()?.Name.ToString();
                var className = classDeclaration.Identifier.Text;
                var properties = classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
                var methodSyntax = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

                //属性集合
                var props = properties.Select(d => new PropertyMetaData(d.Identifier.Text, d.GetAttributeMetaData(), GetPropertyDescription(d), d.Modifiers.ToString(), d.Type?.ToString())).ToList();
                //方法集合
                var methods = methodSyntax.Select(GetMethodMetaData).ToList();
                //实现的接口集合
                var interfaces = classDeclaration.BaseList?.ToString().Split(':').Last().Trim().Split(',').Where(d => d.Split('.').Last().StartsWith("I")).ToList() ?? new List<string>();
                //using 引用
                //特殊处理 class中嵌套class
                var parent = classDeclaration.Parent is ClassDeclarationSyntax
                    ? classDeclaration.Parent?.Parent?.Parent
                    : classDeclaration.Parent?.Parent;
                var usingDirectiveSyntax = parent == null ? new SyntaxList<UsingDirectiveSyntax>() : ((CompilationUnitSyntax)parent).Usings;
                var usingList = usingDirectiveSyntax.Select(d => d.ToString()).ToList();

                //构造函数
                var constructorDictionary = new List<KeyValueModel>();
                foreach (var memberDeclarationSyntax in classDeclaration.Members)
                {
                    if (memberDeclarationSyntax.Kind().ToString() == "ConstructorDeclaration")
                    {
                        //constructorDictionary = memberDeclarationSyntax.DescendantNodes().OfType<ParameterSyntax>().ToDictionary(d => d.GetFirstToken().Text, d => d.GetLastToken().Text);
                        constructorDictionary = memberDeclarationSyntax.DescendantNodes().OfType<ParameterSyntax>().Select(d => new KeyValueModel(d.Type?.ToString(), d.Identifier.Text)).ToList();
                        break;
                    }
                }

                return new ClassMetaData(namespaceName, className, classDeclaration.GetAttributeMetaData(), props, methods, interfaces, constructorDictionary, usingList, classDeclaration.Modifiers.ToString());
            }
            catch (Exception e)
            {
                throw new Exception($"class 报错：{classDeclaration.Identifier.Text}", e);
            }
        }
        
        private MethodMetaData GetMethodMetaData(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            var param = new List<KeyValueModel>();
            var properties = methodDeclarationSyntax.DescendantNodes().OfType<ParameterListSyntax>().FirstOrDefault()?.DescendantNodes().OfType<ParameterSyntax>().ToList() ?? new List<ParameterSyntax>();
            foreach (var parameterSyntax in properties)
            {
                var type = parameterSyntax?.Type?.ToString();
                var name = parameterSyntax?.Identifier.Text;
                if (type != null && name != null)
                    param.Add(new KeyValueModel(type, name));
            }

            var returnValue = methodDeclarationSyntax.ReturnType.ToString();

            return new MethodMetaData(methodDeclarationSyntax.Identifier.Text,
                methodDeclarationSyntax.GetAttributeMetaData(), returnValue, param, methodDeclarationSyntax.Modifiers.ToString(), methodDeclarationSyntax.Modifiers.ToString(),null);
        }

        /// <summary>
        /// 获取属性上的注释
        /// </summary>
        /// <param name="propertyDeclarationSyntax"></param>
        /// <returns></returns>
        private List<string> GetPropertyDescription(PropertyDeclarationSyntax propertyDeclarationSyntax)
        {
            return propertyDeclarationSyntax.DescendantTokens().OfType<SyntaxToken>()
                .Where(t => t.HasLeadingTrivia && t.LeadingTrivia.Any(l => l != null && l.Kind() == SyntaxKind.SingleLineCommentTrivia))
                .SelectMany(t => t.LeadingTrivia.Where(l => l != null && l.Kind() == SyntaxKind.SingleLineCommentTrivia))
                .Select(t => t.ToString())
                .ToList();
        }
    }
}