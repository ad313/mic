using Mic.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Mic.Helpers
{
    /// <summary>
    /// 表达式树辅助类
    /// </summary>
    public class ExpressionHelper
    {
        /// <summary>
        /// 生成 d => d.Name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Expression<Func<T, object>> GetPropertyExpression<T>(string propertyName)
        {
            // d =>
            var parameter = Expression.Parameter(typeof(T), "d");
            // d.Name
            var member = Expression.Property(parameter, propertyName);
            // (object)d.Name
            var convertedExpression = Expression.Convert(member, typeof(object));
            // d => (object)d.Name
            return Expression.Lambda<Func<T, object>>(convertedExpression, parameter);
        }

        /// <summary>
        /// 生成 d => new { d.Name, d.Code }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static Expression<Func<T, object>> GetAnonymousExpression<T>(List<string> columns)
        {
            var entityType = typeof(T);

            var allProps = entityType.GetProperties();
            var props = new List<PropertyInfo>();
            foreach (var name in columns)
            {
                var pro = allProps.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (pro != null)
                    props.Add(pro);
            }

            if (!props.Any())
                throw new ArgumentNullException(nameof(columns));

            var source = Expression.Parameter(entityType, "d");
            // create x=> new myType{ prop1 = x.prop1,...}
            var newType = CreateAnonymousType(props);
            var binding = props.Select(p => Expression.Bind(newType.GetField(p.Name), Expression.Property(source, p.Name))).ToList();
            var body = Expression.MemberInit(Expression.New(newType), binding);
            var selector = Expression.Lambda<Func<T, object>>(body, source);
            return selector;
        }

        /// <summary>
        /// 生成匿名类
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        private static Type CreateAnonymousType(List<PropertyInfo> props)
        {
            var asmName = new AssemblyName(Guid.NewGuid().ToString());
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule(Guid.NewGuid().ToString());
            var dynamicAnonymousType = dynamicModule.DefineType("AnonymousType", TypeAttributes.Public);

            foreach (var p in props)
            {
                dynamicAnonymousType.DefineField(p.Name, p.PropertyType, FieldAttributes.Public);
            }
            return dynamicAnonymousType.CreateType();
        }

        /// <summary>
        /// 创建 Where 表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="column"></param>
        /// <param name="oprator"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetWhereExpression<T>(string column, CompareOprator oprator, object value)
        {
            var parameter = Expression.Parameter(typeof(T), "d");
            var member = Expression.PropertyOrField(parameter, column);
            var valueExpress = Expression.Constant(value);

            //类型转换
            dynamic rightExpress = member.Type == valueExpress.Type ? valueExpress : Expression.Convert(valueExpress, member.Type);

            BinaryExpression compare = null;
            switch (oprator)
            {
                case CompareOprator.Equal:
                    compare = Expression.Equal(member, rightExpress);
                    break;
                case CompareOprator.NotEqual:
                    compare = Expression.NotEqual(member, rightExpress);
                    break;
                case CompareOprator.LessThan:
                    compare = Expression.LessThan(member, rightExpress);
                    break;
                case CompareOprator.LessThanOrEqual:
                    compare = Expression.LessThanOrEqual(member, rightExpress);
                    break;
                case CompareOprator.GreaterThan:
                    compare = Expression.GreaterThan(member, rightExpress);
                    break;
                case CompareOprator.GreaterThanOrEqual:
                    compare = Expression.GreaterThanOrEqual(member, rightExpress);
                    break;
                case CompareOprator.Contains:
                    return GetContainsExpression<T>(column, value, false);
                case CompareOprator.NotContains:
                    return GetContainsExpression<T>(column, value, true);
                case CompareOprator.In:
                    return GetInExpression<T>(column, value, false);
                case CompareOprator.NotIn:
                    return GetInExpression<T>(column, value, true);
                default:
                    throw new ArgumentOutOfRangeException(nameof(oprator), oprator, null);
            }

            return Expression.Lambda<Func<T, bool>>(compare, parameter);
        }

        /// <summary>
        /// Contains
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <param name="ifNot">是否是非包含</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetContainsExpression<T>(string propertyName, object propertyValue, bool ifNot)
        {
            var parameterExp = Expression.Parameter(typeof(T), "d");
            var member = Expression.Property(parameterExp, propertyName);

            var underType = member.Type.UnderlyingSystemType.GetGenericArguments().FirstOrDefault();
            if (underType == null)
            {
                MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var valueExpress = Expression.Constant(propertyValue.ToString(), typeof(string));

                if (method == null)
                    throw new ArgumentNullException(nameof(method));

                var containsMethodExp = Expression.Call(member, method, valueExpress);
                var notNullExpression = GetWhereExpression<T>(propertyName, CompareOprator.NotEqual, null);

                if (ifNot)
                {
                    return Expression.Lambda<Func<T, bool>>(Expression.Not(containsMethodExp), parameterExp);
                }
                else
                {
                    return notNullExpression.And(Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp));
                }
            }
            else
            {
                MethodInfo method = typeof(Enumerable).GetMethods()
                    .FirstOrDefault(info => info.GetParameters().Length == 2 && info.Name == "Contains")
                    ?.MakeGenericMethod(underType);

                if (method == null)
                    throw new ArgumentNullException(nameof(method));

                var valueExpress = Expression.Constant(propertyValue, underType);

                var containsMethodExp = Expression.Call(null, method, member, valueExpress);
                var notNullExpression = GetWhereExpression<T>(propertyName, CompareOprator.NotEqual, null);

                if (ifNot)
                {
                    return Expression.Lambda<Func<T, bool>>(Expression.Not(containsMethodExp), parameterExp);
                }
                else
                {
                    return notNullExpression.And(Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp));
                }
            }
        }

        /// <summary>
        /// Contains
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <param name="ifNot">是否是非包含</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetInExpression<T>(string propertyName, object propertyValue, bool ifNot)
        {
            var parameterExp = Expression.Parameter(typeof(T), "d");
            var member = Expression.Property(parameterExp, propertyName);

            var underType = member.Type;
            MethodInfo method = typeof(Enumerable).GetMethods()
                .FirstOrDefault(info => info.GetParameters().Length == 2 && info.Name == "Contains")
                ?.MakeGenericMethod(underType);

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            var valueExpress = Expression.Constant(propertyValue);
            var containsMethodExp = Expression.Call(null, method, valueExpress, member);

            if (ifNot)
            {
                return Expression.Lambda<Func<T, bool>>(Expression.Not(containsMethodExp), parameterExp);
            }
            else
            {
                return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
            }
        }
    }

    /// <summary>
    /// 比较枚举
    /// </summary>
    public enum CompareOprator
    {
        [Description("等于")]
        Equal,
        [Description("不等于")]
        NotEqual,
        [Description("小于")]
        LessThan,
        [Description("小于等于")]
        LessThanOrEqual,
        [Description("大于")]
        GreaterThan,
        [Description("大于等于")]
        GreaterThanOrEqual,
        [Description("包含")]
        Contains,
        [Description("不包含")]
        NotContains,
        [Description("In")]
        In,
        [Description("NotIn")]
        NotIn,
        [Description("多级Like")]
        MultipleContains,
    }
}
