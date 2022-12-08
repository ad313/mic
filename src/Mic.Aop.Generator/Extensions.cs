using System.Collections.Generic;
using System.Linq;
using Mic.Aop.Generator.MetaData;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mic.Aop.Generator
{
    public static class Extensions
    {
        public static bool HasAttribute(this SyntaxList<AttributeListSyntax> attributes, string name)
        {
            string fullname, shortname;
            var attrLen = "Attribute".Length;
            if (name.EndsWith("Attribute"))
            {
                fullname = name;
                shortname = name.Remove(name.Length - attrLen, attrLen);
            }
            else
            {
                fullname = name + "Attribute";
                shortname = name;
            }

            return attributes.Any(al => al.Attributes.Any(a => a.Name.ToString() == shortname || a.Name.ToString() == fullname));
        }
        
        public static T FindParent<T>(this SyntaxNode node) where T : class
        {
            var current = node;
            while(true)
            {
                current = current.Parent;
                if (current == null || current is T)
                    return current as T;
            }
        }

        /// <summary>
        /// ��ȡ Attribute ����
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static List<AttributeMetaData> GetAttributeMetaData(this MemberDeclarationSyntax property)
        {
            var list = new List<AttributeMetaData>();

            foreach (var attributeListSyntax in property.AttributeLists)
            {
                var attribute = attributeListSyntax.Attributes.FirstOrDefault();
                if (attribute == null)
                    continue;

                //var attributeName = ((IdentifierNameSyntax)attribute.Name).Identifier.Text;
                var attributeName = attribute.Name.ToString();

                var attributeMetaData = new AttributeMetaData(attributeName);

                list.Add(attributeMetaData);

                if (attribute.ArgumentList?.Arguments == null)
                    continue;

                foreach (var argument in attribute.ArgumentList.Arguments)
                {
                    var key = argument.GetFirstToken().ValueText;
                    var value = argument.GetLastToken().ValueText;

                    if (argument.ToString().Contains("="))
                    {
                        var arr = argument.ToString().Split('=');
                        key = arr[0].Trim();
                        value = arr[1].Trim();
                    }

                    if (key == "EnumParam")
                    {

                    }

                    attributeMetaData.AddParam(key, value);
                }
            }

            return list;
        }

        public static string? GetStringParam(this List<AttributeMetaData> attributeMetaData, string attributeName, string key)
        {
            if (!attributeMetaData.Any()) return null;
            return attributeMetaData.FirstOrDefault(d => d.Name == attributeName)?.GetStringParam(key);
        }

        public static int? GetIntParam(this List<AttributeMetaData> attributeMetaData, string attributeName, string key)
        {
            if (!attributeMetaData.Any()) return null;
            return attributeMetaData.FirstOrDefault(d => d.Name == attributeName)?.GetIntParam(key);
        }

        public static bool? GetBoolParam(this List<AttributeMetaData> attributeMetaData, string attributeName, string key)
        {
            if (!attributeMetaData.Any()) return null;
            return attributeMetaData.FirstOrDefault(d => d.Name == attributeName)?.GetBoolParam(key);
        }

        public static bool HasAopAttribute<T>(this T classDeclarationSyntax, List<string> aopAttributeList, string ignoreAttribute) where T : MemberDeclarationSyntax
        {
            foreach (var attribute in aopAttributeList)
            {
                if (classDeclarationSyntax.AttributeLists.HasAttribute(attribute))
                    return true;
            }

            if (classDeclarationSyntax.AttributeLists.HasAttribute(ignoreAttribute))
                return true;

            return false;
        }

        public static bool HasIgnore(this List<AttributeMetaData> attributeMetaDatas,string ignoreAttribute)
        {
            return attributeMetaDatas.Any(d => d.Name == ignoreAttribute || d.Name + "Attribute" == ignoreAttribute);
        }

        public static bool HasIgnore(this ClassMetaData classMetaData, string ignoreAttribute)
        {
            return classMetaData.AttributeMetaData.HasIgnore(ignoreAttribute);
        }

        public static bool HasIgnore(this InterfaceMetaData interfaceMetaData, string ignoreAttribute)
        {
            return interfaceMetaData.AttributeMetaData.HasIgnore(ignoreAttribute);
        }

        public static AttributeMetaData? GetAopAttribute(this List<AttributeMetaData> attributeMetaDatas, List<string> aopAttributeList)
        {
            return attributeMetaDatas.FirstOrDefault(d => aopAttributeList.Contains(d.Name) || aopAttributeList.Contains(d.Name + "Attribute"));
        }

        public static List<AttributeMetaData> GetAopAttributes(this List<AttributeMetaData> attributeMetaDatas, List<string> aopAttributeList)
        {
            return attributeMetaDatas.Where(d => aopAttributeList.Contains(d.Name.Replace("Attribute", "") + "Attribute")).ToList();
        }
    }
}