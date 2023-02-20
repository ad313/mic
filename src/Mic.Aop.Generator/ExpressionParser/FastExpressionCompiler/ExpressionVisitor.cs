﻿//// <auto-generated/>
///*
//The MIT License (MIT)

//Copyright (c) 2016-2023 Maksim Volkau

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.
//*/

//#if LIGHT_EXPRESSION || !NET45
//#define SUPPORTS_ARGUMENT_PROVIDER
//#endif

//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;

//#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
//#nullable disable

//namespace FastExpressionCompiler.LightExpression
//{
//    /// <summary>The order of the visiting is important</summary>
//    public abstract class ExpressionVisitor
//    {
//        public virtual Expression Visit(Expression node) => node?.Accept(this);

//        public IReadOnlyList<Expression> Visit(IReadOnlyList<Expression> nodes)
//        {
//            Expression[] newNodes = null;
//            var count = nodes.Count;
//            for (var i = 0; i < count; ++i)
//            {
//                var newNode = Visit(nodes[i]);
//                if (newNodes != null)
//                    newNodes[i] = newNode;
//                else if (newNode != nodes[i])
//                {
//                    newNodes = new Expression[count];
//                    for (var j = 0; j < i; ++j)
//                        newNodes[j] = nodes[j];
//                    newNodes[i] = newNode;
//                }
//            }

//            return newNodes ?? nodes;
//        }

//        public IReadOnlyList<T> VisitAndConvert<T>(IReadOnlyList<T> nodes) where T : Expression
//        {
//            T[] newNodes = null;
//            var count = nodes.Count;
//            for (var i = 0; i < count; ++i)
//            {
//                var newNode = VisitAndConvert(nodes[i]);
//                if (newNodes != null)
//                    newNodes[i] = newNode;
//                else if (newNode != nodes[i])
//                {
//                    newNodes = new T[count];
//                    for (var j = 0; j < i; ++j)
//                        newNodes[j] = nodes[j];
//                    newNodes[i] = newNode;
//                }
//            }
//            return newNodes ?? nodes;
//        }

//        public IReadOnlyList<T> VisitAndConvert<T>(IReadOnlyList<T> nodes, Func<T, T> visit)
//        {
//            T[] newNodes = null;
//            var count = nodes.Count;
//            for (var i = 0; i < count; ++i)
//            {
//                var newNode = visit(nodes[i]);
//                if (newNodes != null)
//                    newNodes[i] = newNode;
//                else if (!Equals(newNode, nodes[i]))
//                {
//                    newNodes = new T[count];
//                    for (var j = 0; j < i; ++j)
//                        newNodes[j] = nodes[j];
//                    newNodes[i] = newNode;
//                }
//            }
//            return newNodes ?? nodes;
//        }

//        internal IReadOnlyList<T> VisitAndConvertArguments<T>(IReadOnlyList<T> nodes, Func<T, T> visit)
//        {
//            T[] newNodes = null;
//            var count = nodes.Count;
//            for (var i = 0; i < count; ++i)
//            {
//                var newNode = visit(nodes[i]);
//                if (newNodes != null)
//                    newNodes[i] = newNode;
//                else if (!Equals(newNode, nodes[i]))
//                {
//                    newNodes = new T[count];
//                    for (var j = 0; j < i; ++j)
//                        newNodes[j] = nodes[j];
//                    newNodes[i] = newNode;
//                }
//            }
//            return newNodes;
//        }

//        internal IReadOnlyList<ParameterExpression> VisitAndConvertParameters(IParameterProvider nodes)
//        {
//            ParameterExpression[] newNodes = null;
//            var count = nodes.ParameterCount;
//            for (var i = 0; i < count; ++i)
//            {
//                var node = nodes.GetParameter(i);
//                var newNode = VisitAndConvert(node);
//                if (newNodes != null)
//                    newNodes[i] = newNode;
//                else if (!Equals(newNode, node))
//                {
//                    newNodes = new ParameterExpression[count];
//                    for (var j = 0; j < i; ++j)
//                        newNodes[j] = nodes.GetParameter(j);
//                    newNodes[i] = newNode;
//                }
//            }
//            return newNodes;
//        }

//        internal IReadOnlyList<T> VisitAndConvertArguments<T>(IArgumentProvider<T> nodes, Func<T, T> visit)
//        {
//            T[] newNodes = null;
//            var count = nodes.ArgumentCount;
//            for (var i = 0; i < count; ++i)
//            {
//                var node = nodes.GetArgument(i);
//                var newNode = visit(node);
//                if (newNodes != null)
//                    newNodes[i] = newNode;
//                else if (!Equals(newNode, node))
//                {
//                    newNodes = new T[count];
//                    for (var j = 0; j < i; ++j)
//                        newNodes[j] = nodes.GetArgument(j);
//                    newNodes[i] = newNode;
//                }
//            }
//            return newNodes;
//        }

//        /// <summary>Returns null if the visited nodes are no different</summary>
//        internal Expression[] VisitArguments(IReadOnlyList<Expression> nodes)
//        {
//            Expression[] newNodes = null;
//            var count = nodes.Count;
//            for (var i = 0; i < count; ++i)
//            {
//                var newNode = Visit(nodes[i]);
//                if (newNodes != null)
//                    newNodes[i] = newNode;
//                else if (newNode != nodes[i])
//                {
//                    newNodes = new Expression[count];
//                    for (var j = 0; j < i; ++j)
//                        newNodes[j] = nodes[j];
//                    newNodes[i] = newNode;
//                }
//            }

//            return newNodes;
//        }

//        /// <summary>Returns null if the visited nodes are no different</summary>
//        public Expression[] VisitArguments(IArgumentProvider args)
//        {
//            Expression[] newNodes = null;
//            var count = args.ArgumentCount;
//            for (var i = 0; i < count; ++i)
//            {
//                var node = args.GetArgument(i);
//                var newNode = Visit(node);
//                if (newNodes != null)
//                    newNodes[i] = newNode;
//                else if (!object.ReferenceEquals(newNode, node))
//                {
//                    newNodes = new Expression[count];
//                    for (var j = 0; j < i; ++j)
//                        newNodes[j] = args.GetArgument(j);
//                    newNodes[i] = newNode;
//                }
//            }
//            return newNodes;
//        }
        
//        public T VisitAndConvert<T>(T node) where T : Expression
//        {
//            if (node == null)
//                return null;
//            var x = Visit(node);
//            if (x is T converted)
//                return converted;
//            throw new InvalidOperationException($"Converting visited node is not compatible from {x?.GetType()} to {typeof(T)}");
//        }

//        protected internal virtual Expression VisitBinary(BinaryExpression node)
//        {
//            var left = Visit(node.Left); 
//            var conversion = VisitAndConvert(node.Conversion);
//            var right = Visit(node.Right);
//            if (node.Left == left && node.Conversion == conversion && node.Right == right)
//                return node;
//            return Expression.MakeBinary(node.NodeType, left, right, conversion);
//        }

//        protected internal virtual Expression VisitBlock(BlockExpression node)
//        {
//            // the order of the visiting is important
//            var expressions = Visit(node.Expressions);
//            var variables = VisitAndConvert(node.Variables);
//            if (ReferenceEquals(expressions, node.Expressions) && ReferenceEquals(variables, node.Variables))
//                return node;
//            return Expression.MakeBlock(node.Type, variables, expressions);
//        }

//        protected internal virtual Expression VisitConditional(ConditionalExpression node)
//        {
//            var test = Visit(node.Test);
//            var ifTrue = Visit(node.IfTrue);
//            var ifFalse = Visit(node.IfFalse);
//            if (test == node.Test && ifTrue == node.IfTrue && ifFalse == node.IfFalse)
//                return node;
//            if (ifFalse == Expression.VoidDefault)
//                return Expression.IfThen(test, ifTrue);
//            return Expression.Condition(test, ifTrue, ifFalse, node.Type);
//        }

//        protected internal virtual Expression VisitConstant(ConstantExpression node) => node;

//        protected internal virtual Expression VisitDefault(DefaultExpression node) => node;

//        protected internal virtual LabelTarget VisitLabelTarget(LabelTarget node) => node;

//        protected internal virtual Expression VisitGoto(GotoExpression node)
//        {
//            var target = VisitLabelTarget(node.Target);
//            var value = Visit(node.Value);
//            if (target == node.Target && value == node.Value)
//                return node;
//            return Expression.Goto(target, value, node.Type);
//        }

//        protected internal virtual Expression VisitInvocation(InvocationExpression node)
//        {
//            var expression = Visit(node.Expression);
//#if SUPPORTS_ARGUMENT_PROVIDER
//            var arguments = VisitArguments((IArgumentProvider)node);
//#else            
//            var arguments = VisitArguments(node.Arguments);
//#endif
//            if (expression == node.Expression && arguments == null)
//                return node;
//            return Expression.Invoke(node.Type, expression, arguments);
//        }

//        protected internal virtual Expression VisitLabel(LabelExpression node)
//        {
//            var target = VisitLabelTarget(node.Target);
//            var value = Visit(node.DefaultValue);
//            if (target == node.Target && value == node.DefaultValue)
//                return node;
//            return Expression.Label(target, value);
//        }

//        protected internal virtual Expression VisitLambda(LambdaExpression node)
//        {
//            var body = Visit(node.Body);
//#if LIGHT_EXPRESSION
//            var parameters = VisitAndConvertParameters((IParameterProvider)node);
//#else
//            var parameters = VisitAndConvertParameters((IParameterProvider)node);
//#endif
//            if (body == node.Body && parameters == null)
//                return node;
//            return Expression.Lambda(node.Type, body, parameters, node.ReturnType); 
//        }

//        protected internal virtual Expression VisitLambda<T>(Expression<T> node) where T : System.Delegate
//        {
//            var body = Visit(node.Body);
//            var parameters = VisitAndConvert(node.Parameters);
//            if (body == node.Body && ReferenceEquals(parameters, node.Parameters))
//                return node;
//            return Expression.Lambda<T>(body, parameters, node.ReturnType);
//        }

//        protected internal virtual Expression VisitLoop(LoopExpression node)
//        {
//            var breakLabel = VisitLabelTarget(node.BreakLabel);
//            var continueLabel = VisitLabelTarget(node.ContinueLabel);
//            var body = Visit(node.Body);
//            if (breakLabel == node.BreakLabel && continueLabel == node.ContinueLabel && body == node.Body)
//                return node;
//            return Expression.Loop(body, breakLabel, continueLabel);
//        }

//        protected internal virtual Expression VisitMember(MemberExpression node)
//        {
//            var expression = Visit(node.Expression);
//            if (expression == node.Expression)
//                return node;
//            return Expression.MakeMemberAccess(expression, node.Member);
//        }

//        protected internal virtual Expression VisitIndex(IndexExpression node)
//        {
//            var instance = Visit(node.Object);
//#if SUPPORTS_ARGUMENT_PROVIDER
//            var arguments = VisitArguments((IArgumentProvider)node);
//#else            
//            var arguments = VisitArguments(node.Arguments);
//#endif
//            if (instance == node.Object && arguments == null)
//                return node;
//            return Expression.MakeIndex(instance, node.Indexer, arguments);
//        }

//        protected internal virtual Expression VisitMethodCall(MethodCallExpression node)
//        {
//            var instance = Visit(node.Object);
//#if SUPPORTS_ARGUMENT_PROVIDER
//            var arguments = VisitArguments((IArgumentProvider)node);
//#else            
//            var arguments = VisitArguments(node.Arguments);
//#endif
//            return instance == node.Object && arguments == null ? node :
//                Expression.Call(instance, node.Method, arguments);
//        }

//        protected internal virtual Expression VisitNewArray(NewArrayExpression node)
//        {
//#if LIGHT_EXPRESSION
//            var expressions = VisitArguments((IArgumentProvider)node);
//#else
//            var expressions = VisitArguments(node.Expressions);
//#endif
//            if (expressions == null)
//                return node;

//            if (node.NodeType == ExpressionType.NewArrayInit)
//                return new ManyElementsNewArrayInitExpression(node.Type, expressions);
//            return new ManyBoundsNewArrayBoundsExpression(node.Type, expressions);
//        }

//        protected internal virtual Expression VisitNew(NewExpression node)
//        {
//#if SUPPORTS_ARGUMENT_PROVIDER
//            var arguments = VisitArguments((IArgumentProvider)node);
//#else            
//            var arguments = VisitArguments(node.Arguments);
//#endif
//            return arguments == null ? node : Expression.New(node.Constructor, arguments);
//        }

//        protected internal virtual Expression VisitParameter(ParameterExpression node) => node;

//        protected internal virtual SwitchCase VisitSwitchCase(SwitchCase node)
//        {
//            var testValues = Visit(node.TestValues);
//            var body = Visit(node.Body);
//            if (ReferenceEquals(testValues, node.TestValues) && body == node.Body)
//                return node;
//            return new SwitchCase(body, testValues);
//        }

//        protected internal virtual Expression VisitSwitch(SwitchExpression node)
//        {
//            var switchValue = Visit(node.SwitchValue);
//            var cases = VisitAndConvert(node.Cases, VisitSwitchCase);
//            var defaultBody = Visit(node.DefaultBody);
//            if (switchValue == node.SwitchValue && ReferenceEquals(cases, node.Cases) && defaultBody == node.DefaultBody)
//                return node;
//            return Expression.Switch(node.Type, switchValue, defaultBody, node.Comparison, cases.AsArray());
//        }

//        protected internal virtual CatchBlock VisitCatchBlock(CatchBlock node)
//        {
//            var variable = VisitAndConvert(node.Variable);
//            var filter = Visit(node.Filter);
//            var body = Visit(node.Body);
//            if (variable == node.Variable && filter == node.Filter && body == node.Body)
//                return node;
//            return new CatchBlock(node.Test, variable, body, filter);
//        }

//        protected internal virtual Expression VisitTry(TryExpression node)
//        {
//            var body = Visit(node.Body);
            
//            var handlers = node.Handlers?.Count > 0
//                ? VisitAndConvert(node.Handlers, VisitCatchBlock)
//                : node.Handlers;
            
//            var @finally = node.Finally != null 
//                ? Visit(node.Finally) 
//                : node.Finally;

//            if (body == node.Body && ReferenceEquals(handlers, node.Handlers) && @finally == node.Finally)
//                return node;

//            return @finally == null
//                ? new TryExpression(body, handlers.AsArray())
//                : new WithFinallyTryExpression(body, handlers.AsArray(), @finally);
//        }

//        protected internal virtual Expression VisitTypeBinary(TypeBinaryExpression node)
//        {
//            var expression = Visit(node.Expression);
//            if (expression == node.Expression)
//                return node;
//            return new TypeBinaryExpression(node.NodeType, expression, node.TypeOperand);
//        }

//        protected internal virtual Expression VisitUnary(UnaryExpression node)
//        {
//            var operand = Visit(node.Operand);
//            if (operand == node.Operand)
//                return node;
//            return Expression.MakeUnary(node.NodeType, operand, node.Type);
//        }

//        protected internal virtual Expression VisitMemberInit(MemberInitExpression node)
//        {
//            var newExpression = VisitAndConvert(node.NewExpression);
//#if LIGHT_EXPRESSION
//            var bindings = VisitAndConvertArguments((IArgumentProvider<MemberBinding>)node, VisitMemberBinding);
//#else
//            var bindings = VisitAndConvertArguments(node.Bindings, VisitMemberBinding);
//#endif
//            if (newExpression == node.NewExpression && bindings == null)
//                return node;
//            return Expression.MemberInit(newExpression, bindings);
//        }

//        protected internal virtual MemberBinding VisitMemberBinding(MemberBinding node)
//        {
//            switch (node.BindingType)
//            {
//                case MemberBindingType.Assignment:
//                    return VisitMemberAssignment((MemberAssignment)node);
//                case MemberBindingType.MemberBinding:
//                    return VisitMemberMemberBinding((MemberMemberBinding)node);
//                case MemberBindingType.ListBinding:
//                    return VisitMemberListBinding((MemberListBinding)node);
//                default:
//                    throw new NotSupportedException($"Unhandled Binding Type: {node.BindingType}");
//            }
//        }

//        protected internal virtual MemberAssignment VisitMemberAssignment(MemberAssignment node)
//        {
//            var expression = Visit(node.Expression);
//            if (expression == node.Expression)
//                return node;
//            return new MemberAssignment(node.Member, expression);
//        }
        
//        protected internal virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
//        {
//            var bindings = VisitAndConvert(node.Bindings, b => VisitMemberBinding(b));
//            if (ReferenceEquals(bindings, node.Bindings))
//                return node;
//            return new MemberMemberBinding(node.Member, bindings);
//        }

//        protected internal virtual MemberListBinding VisitMemberListBinding(MemberListBinding node)
//        {
//            var newItems = VisitAndConvert(node.Initializers, x => VisitElementInit(x));
//            if (ReferenceEquals(newItems, node.Initializers))
//                return node;
//            return new MemberListBinding(node.Member, newItems);
//        }

//        protected internal virtual Expression VisitListInit(ListInitExpression node)
//        {
//            var newExpression = VisitAndConvert(node.NewExpression);
//            var initializers  = VisitAndConvert(node.Initializers, VisitElementInit);
//            if (newExpression == node.NewExpression && ReferenceEquals(initializers, node.Initializers))
//                return node;
//            return new ListInitExpression(newExpression, initializers.AsReadOnlyList());
//        }

//        protected internal virtual ElementInit VisitElementInit(ElementInit node) 
//        {
//#if SUPPORTS_ARGUMENT_PROVIDER
//            var arguments = VisitArguments((IArgumentProvider)node);
//#else
//            var arguments = VisitArguments(node.Arguments);
//#endif
//            if (arguments == null)
//                return node;
//            return Expression.ElementInit(node.AddMethod, arguments);
//        }

//        protected internal virtual Expression VisitDynamic(DynamicExpression node) => node;

//        protected internal virtual Expression VisitExtension(Expression node) =>
//            node.VisitChildren(this);

//        protected internal virtual Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
//        {
//            var newItems = VisitAndConvert(node.Variables);
//            if (newItems == node.Variables)
//                return node;
//            return new RuntimeVariablesExpression(newItems);
//        }

//        protected internal virtual Expression VisitDebugInfo(DebugInfoExpression node) => node;
//    }
//}
