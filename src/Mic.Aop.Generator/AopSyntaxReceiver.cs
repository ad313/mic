using System.Collections.Generic;
using System.Linq;
using Mic.Aop.Generator.MetaData;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        private readonly List<ClassDeclarationSyntax> _classSyntaxList = new List<ClassDeclarationSyntax>();
        /// <summary>
        /// 接口列表
        /// </summary>
        private readonly List<InterfaceDeclarationSyntax> _interfaceSyntaxList = new List<InterfaceDeclarationSyntax>();
        /// <summary>
        /// 所有的AopInterceptor
        /// </summary>
        public List<string> AopAttributeList = new List<string>();
        /// <summary>
        /// 所有的AopInterceptor
        /// </summary>
        public List<ClassMetaData> AopAttributeClassMetaDataList = new List<ClassMetaData>();

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
        /// 找出所有 AopInterceptor 
        /// </summary>
        /// <returns></returns>
        public AopSyntaxReceiver FindAopInterceptor()
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
                            if (meta != null && AopAttributeClassMetaDataList.All(d => d.Name != meta.Name))
                                AopAttributeClassMetaDataList.Add(meta);
                        }
                    }
                }
            }

            AopAttributeList = AopAttributeList.Distinct().ToList();

            return this;
        }

        /// <summary>
        /// 获取所有标记了 AopInterceptor 的接口和类
        /// </summary>
        /// <param name="compilation"></param>
        /// <returns></returns>
        public AopMetaData GetAopMetaData(Compilation compilation)
        {
            var result = new AopMetaData(AopAttributeList, IgnoreAttribute, new List<InterfaceMetaData>(), new List<ClassMetaData>());

            if (!AopAttributeList.Any())
                return result;

            //处理接口
            foreach (var classSyntax in this._interfaceSyntaxList)
            {
                var root = classSyntax.SyntaxTree.GetRoot();
                var interfaceWithAttribute = root
                    .DescendantNodes()
                    .OfType<InterfaceDeclarationSyntax>()
                    .ToList();

                if (!interfaceWithAttribute.Any())
                    continue;

                foreach (var interfaceDeclaration in interfaceWithAttribute)
                {
                    var namespaceName = interfaceDeclaration.FindParent<NamespaceDeclarationSyntax>().Name.ToString();
                    var className = interfaceDeclaration.Identifier.Text;
                    var properties = interfaceDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
                    var methodSyntaxs = interfaceDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

                    ////接口上没有标记，则跳过
                    //if (!interfaceDeclaration.HasAopAttribute(AopAttributeList, IgnoreAttribute) && methodSyntaxs.All(d => !d.HasAopAttribute(AopAttributeList, IgnoreAttribute)))
                    //    continue;

                    //属性集合
                    var props = properties.Select(d => new PropertyMetaData(d.Identifier.Text, d.GetAttributeMetaData())).ToList();
                    //方法集合
                    var methods = methodSyntaxs.Select(GetMethodMetaData).ToList();

                    var interfaceMetaData = new InterfaceMetaData(namespaceName, className, interfaceDeclaration.GetAttributeMetaData(), props, methods);
                    if (interfaceMetaData.MethodMetaData.Any() && !result.InterfaceMetaDataList.Exists(d => d.Equals(interfaceMetaData)))
                        result.InterfaceMetaDataList.Add(interfaceMetaData);
                }
            }

            //处理类
            foreach (var classSyntax in this._classSyntaxList)
            {
                var root = classSyntax.SyntaxTree.GetRoot();
                var classesWithAttribute = root
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .ToList();

                if (!classesWithAttribute.Any())
                    continue;

                foreach (var classDeclaration in classesWithAttribute)
                {
                    var classMetaData = GetClassMetaData(classDeclaration);
                    if (classMetaData == null)
                        continue;

                    if (AopAttributeList.Contains(classMetaData.Name))
                        continue;

                    if (classMetaData.MethodMetaData.Any() && !result.ClassMetaDataList.Exists(d => d.Equals(classMetaData)))
                        result.ClassMetaDataList.Add(classMetaData);
                }
            }

            result.AopAttributeClassMetaDataList = AopAttributeClassMetaDataList;

            return result;
        }

        private ClassMetaData? GetClassMetaData(ClassDeclarationSyntax classDeclaration)
        {
            var namespaceName = classDeclaration.FindParent<NamespaceDeclarationSyntax>().Name.ToString();
            var className = classDeclaration.Identifier.Text;
            var properties = classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
            var methodSyntaxs = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

            //属性集合
            var props = properties.Select(d => new PropertyMetaData(d.Identifier.Text, d.GetAttributeMetaData())).ToList();
            //方法集合
            var methods = methodSyntaxs.Select(GetMethodMetaData).ToList();
            //实现的接口集合
            var interfaces = classDeclaration.BaseList?.ToString().Split(':').Last().Trim().Split(',').Where(d => d.Split('.').Last().StartsWith("I")).ToList() ?? new List<string>();
            //using 引用
            var usingDirectiveSyntax = classDeclaration.Parent?.Parent == null ? new SyntaxList<UsingDirectiveSyntax>() : ((CompilationUnitSyntax)classDeclaration.Parent.Parent).Usings;
            var usings = usingDirectiveSyntax.Select(d => d.ToString()).ToList();

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

            return new ClassMetaData(namespaceName, className, classDeclaration.GetAttributeMetaData(), props, methods, interfaces, constructorDictionary, usings);
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
                methodDeclarationSyntax.GetAttributeMetaData(), returnValue, param, methodDeclarationSyntax.Modifiers.ToString());
        }
    }
}