﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace CouchDB.Driver.Query {
    using CouchDB.Driver.Query.Extensions;
    using CouchDB.Driver.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Security.Authentication;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Code originates from couchdb.client project.
    /// </summary>
    internal class ExpressionToMango : ExpressionVisitor {

        /// <summary>
        /// Create translator
        /// </summary>
        private ExpressionToMango() {
            _sb = new StringBuilder();
        }

        /// <summary>
        /// Check that expression can be translated
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsValid(Expression e) {
            var translator = new ExpressionToMango();
            try {
                translator.Visit(e);
                return true;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Translate to mango
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string Translate(Expression e) {
            e = new QueryOptimizer().Optimize(e);
            var translator = new ExpressionToMango();
            return translator.Process(e);
        }

        private string Process(Expression e) {
            _sb.Clear();
            _sb.Append('{');
            Visit(e);

            // If no Where() calls
            if (!_isSelectorSet) {
                // If no other methods calls - ToList()
                if (_sb.Length > 1) {
                    _sb.Length--;
                    _sb.Append(',');
                }
                _sb.Append("\"selector\":{}");
            }
            else {
                _sb.Length--;
            }

            _sb.Append('}');
            var body = _sb.ToString();
            return body;
        }

        /// <inheritdoc/>
        protected override Expression VisitMember(MemberExpression m) {

            if (m.Expression is ConstantExpression ce) {
                VisitConstantValue(ce.Value);
            }
            else {
                var propName = "." + m.Member.GetPropertyName();
                var currentExpression = m.Expression;
                while (true) {
                    if (currentExpression is MemberExpression cm) {
                        propName = "." + cm.Member.GetPropertyName() + propName;
                        currentExpression = cm.Expression;
                    }
                    else if (currentExpression is BinaryExpression be &&
                        be.NodeType == ExpressionType.ArrayIndex &&
                        be.Right is ConstantExpression i) {
#if FALSE
                        propName = $"[{i.Value}]" + propName;
                        currentExpression = be.Left;
#else
                        throw new NotSupportedException("Array Index not supported");
#endif
                    }
                    else if (currentExpression.NodeType != ExpressionType.Parameter) {
                        throw new NotSupportedException(
                            $"Expression {currentExpression} not supported");
                    }
                    else {
                        break;
                    }
                }

                _sb.Append($"\"{propName.TrimStart('.')}\"");
            }
            return m;
        }

        /// <inheritdoc/>
        protected override Expression VisitLambda<T>(Expression<T> l) {
            Visit(l.Body);
            return l;
        }

        /// <inheritdoc/>
        protected override Expression VisitBinary(BinaryExpression b) {

            void VisitBinaryComparisonOperator(ExpressionType type, Expression b) {
                _sb.Append('{');
                switch (type) {
                    case ExpressionType.NotEqual:
                        _sb.Append("\"$ne\":");
                        break;
                    case ExpressionType.LessThan:
                        _sb.Append("\"$lt\":");
                        break;
                    case ExpressionType.LessThanOrEqual:
                        _sb.Append("\"$lte\":");
                        break;
                    case ExpressionType.GreaterThan:
                        _sb.Append("\"$gt\":");
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        _sb.Append("\"$gte\":");
                        break;
                    default:
                        throw new NotSupportedException(
                            $"The binary operator '{type}' is not supported");
                }
                Visit(b);
                _sb.Append('}');
            }

            _sb.Append('{');
            switch (b.NodeType) {
                case ExpressionType.Equal:
                    switch (b.Left) {
                        // $size operator = list.Count == size
                        case MemberExpression m
                            when m.Member.Name == nameof(ICollection.Count) &&
                                typeof(ICollection).IsAssignableFrom(m.Member.DeclaringType):
                            Visit(m.Expression);
                            _sb.Append(":{\"$size\":");
                            Visit(b.Right);
                            _sb.Append("}}");
                            return b;
                        // $size operator = list.Count == size
                        case UnaryExpression m
                            when m.NodeType == ExpressionType.ArrayLength:
                            Visit(m.Operand);
                            _sb.Append(":{\"$size\":");
                            Visit(b.Right);
                            _sb.Append("}}");
                            return b;
                        // $mod operator = prop % divisor = remainder
                        case BinaryExpression mb
                            when mb.NodeType == ExpressionType.Modulo:
                            if (mb.Left is not MemberExpression c ||
                                c.Member is not PropertyInfo r ||
                                r.PropertyType != typeof(int)) {
                                throw new NotSupportedException(
                                    "The document field must be an integer.");
                            }
                            Visit(mb.Left);
                            _sb.Append(":{\"$mod\":[");
                            Visit(mb.Right);
                            _sb.Append(',');
                            Visit(b.Right);
                            _sb.Append("]}}");
                            return b;
                        default:
                            Visit(b.Left);
                            break;
                    }
                    _sb.Append(':');
                    Visit(b.Right);
                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    VisitBinaryCombinationOperator(b, false);
                    break;
                default:
                    switch (b.Left) {
                        // $size operator = list.Count == size
                        case MemberExpression m
                            when m.Member.Name == nameof(ICollection.Count) &&
                                typeof(ICollection).IsAssignableFrom(m.Member.DeclaringType):
                            throw new NotSupportedException(
                        "List length comparison operator other than equal not supported");
#if FALSE
                            Visit(m.Expression);
                            _sb.Append(":{\"$size\":");
                            VisitBinaryComparisonOperator(b.NodeType, b.Right);
                            _sb.Append("}}");
                            return b;
#endif
                        // $size operator = list.Count == size
                        case UnaryExpression m
                            when m.NodeType == ExpressionType.ArrayLength:
                            throw new NotSupportedException(
                        "Array length comparison with operator other than equal not supported");
#if FALSE
                            Visit(m.Operand);
                            _sb.Append(":{\"$size\":");
                            VisitBinaryComparisonOperator(b.NodeType, b.Right);
                            _sb.Append("}}");
                            return b;
#endif
                        // $mod operator = prop % divisor = remainder
                        case BinaryExpression mb
                            when mb.NodeType == ExpressionType.Modulo:
                            throw new NotSupportedException(
                        "Modulo comparison operator other than equal not supported");
#if FALSE
                            if (!(mb.Left is MemberExpression c) ||
                                !(c.Member is PropertyInfo r) ||
                                r.PropertyType != typeof(int)) {
                                throw new NotSupportedException(
                                    "The document field must be an integer.");
                            }
                            Visit(mb.Left);
                            _sb.Append(":{\"$mod\":[");
                            Visit(mb.Right);
                            _sb.Append(",");
                            VisitBinaryComparisonOperator(b.NodeType, b.Right);
                            _sb.Append("]}}");
                            return b;
#endif
                        default:
                            Visit(b.Left);
                            _sb.Append(':');
                            VisitBinaryComparisonOperator(b.NodeType, b.Right);
                            break;
                    }
                    break;
            }
            _sb.Append('}');
            return b;
        }

        /// <inheritdoc/>
        protected override Expression VisitConstant(ConstantExpression c) {
            VisitConstantValue(c.Value);
            return c;
        }

        /// <inheritdoc/>
        protected override Expression VisitExtension(Expression x) {
            return x;
        }

        /// <inheritdoc/>
        protected override Expression VisitUnary(UnaryExpression u) {
            switch (u.NodeType) {
                case ExpressionType.Not:
                    switch (u.Operand) {
                        case BinaryExpression b
                            when b.NodeType == ExpressionType.Or ||
                                 b.NodeType == ExpressionType.OrElse:
                            _sb.Append('{');
                            VisitBinaryCombinationOperator(b, true);
                            _sb.Append('}');
                            break;
                        case MethodCallExpression m
                            when m.Method.Name == "In":
                            _sb.Append('{');
                            Visit(m.Arguments[0]);
                            _sb.Append(":{\"$in\":");
                            Visit(m.Arguments[1]);
                            _sb.Append("}}");
                            break;
                        default:
                            _sb.Append('{');
                            _sb.Append("\"$not\":");
                            Visit(u.Operand);
                            _sb.Append('}');
                            break;
                    }
                    break;
                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(
                        $"The unary operator '{u.NodeType}' is not supported");
            }
            return u;
        }

        /// <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression m) {

            var genericDefinition = m.Method.IsGenericMethod
                ? m.Method.GetGenericMethodDefinition()
                : null;

            // Queryable
            if (genericDefinition == Method.Where) {
                // Optimizer merged all where methods into a single one so this is working
                Visit(m.Arguments[0]);
                _sb.Append("\"selector\":");
                var lambdaBody = m.GetLambda().Body;
                Visit(lambdaBody);
                _sb.Append(',');
                _isSelectorSet = true;
                return m;
            }

            // Ordering
            if (genericDefinition == Method.OrderBy ||
                genericDefinition == Method.ThenBy) {
                return VisitOrderAscendingMethod(m);
            }
            if (genericDefinition == Method.OrderByDescending ||
                genericDefinition == Method.ThenByDescending) {
                return VisitOrderDescendingMethod(m);
            }

            // Limit
            if (genericDefinition == Method.Take) {
                Visit(m.Arguments[0]);
                _sb.Append($"\"limit\":{m.Arguments[1]},");
                return m;
            }

            // Project
            if (genericDefinition == Method.Select) {
                Visit(m.Arguments[0]);
                var lambda = m.GetLambda();
                if (lambda.ReturnType == lambda.Parameters[0].Type) {
                    // Select f
                    return m;
                }
                // Create field projection
                _sb.Append("\"fields\":[");
                if (lambda.Body is NewExpression n) {
                    foreach (var a in n.Arguments) {
                        Visit(a);
                        _sb.Append(',');
                    }
                    _sb.Length--;
                }
                else if (lambda.Body is MemberExpression mb) {
                    Visit(mb);
                }
                else {
                    throw new NotSupportedException(
                        $"The expression of type {lambda.Body} is not supported.");
                }
                _sb.Append("],");
                return m;
            }

            // Any (selector)
            if (genericDefinition == Method.AnyWithPredicate) {
                throw new NotSupportedException("Todo");
#if FALSE // TODO
                _sb.Append("{");
                Visit(m.Arguments[0]);
                _sb.Append(":{\"$elemMatch\":");
                var lambdaBody = m.GetLambdaBody();
                Visit(lambdaBody);
                _sb.Append("}}");
                return m;
#endif
            }

            if (genericDefinition == Method.All) {
                throw new NotSupportedException("Todo");
#if FALSE // TODO
                _sb.Append("{");
                Visit(m.Arguments[0]);
                _sb.Append(":{\"$allMatch\":");
                var lambdaBody = m.GetLambdaBody();
                Visit(lambdaBody);
                _sb.Append("}}");
                return m;
#endif
            }

            // IEnumerable extensions
            if (genericDefinition == Method.EnumerableContains) {
                _sb.Append('{');
                Visit(m.Arguments[0]);
                _sb.Append(":{\"$all\":");
                Visit(m.Arguments[1]);
                _sb.Append("}}");
                return m;
            }

            // String extensions
            if (m.Method == Method.IsMatch) {
                _sb.Append('{');
                Visit(m.Arguments[0]);
                _sb.Append(":{\"$regex\":");
                Visit(m.Arguments[1]);
                _sb.Append("}}");
                return m;
            }

            // List and Enumerable
            if (m.Method == Method.Contains ||
                (m.Method.Name == nameof(List<object>.Contains) &&
                m.Method.DeclaringType.IsGenericType &&
                m.Method.DeclaringType.GetGenericTypeDefinition() == typeof(List<>))) {
                _sb.Append('{');
                Visit(m.Object);
                _sb.Append(":{\"$all\":[");
                Visit(m.Arguments[0]);
                _sb.Append("]}}");
                return m;
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }

        private Expression VisitOrderAscendingMethod(Expression m) {
            void InspectOrdering(Expression e) {
                var o = e as MethodCallExpression ??
                    throw new AuthenticationException(
                        $"Invalid expression type {e.GetType().Name}");
                var lambdaBody = o.GetLambda().Body;

                switch (o.Method.Name) {
                    case "OrderBy":
                        Visit(o.Arguments[0]);
                        _sb.Append("\"sort\":[");
                        Visit(lambdaBody);
                        break;
                    case "OrderByDescending":
                        throw new InvalidOperationException(
                            "Cannot order in different directions.");
                    case "ThenBy":
                        InspectOrdering(o.Arguments[0]);
                        Visit(lambdaBody);
                        break;
                    default:
                        return;
                }
                _sb.Append(',');
            }

            InspectOrdering(m);
            _sb.Length--;
            _sb.Append("],");
            return m;
        }

        private Expression VisitOrderDescendingMethod(Expression m) {
            void InspectOrdering(Expression e) {
                var o = e as MethodCallExpression ??
                    throw new AuthenticationException(
                        $"Invalid expression type {e.GetType().Name}");
                var lambdaBody = o.GetLambda().Body;

                switch (o.Method.Name) {
                    case "OrderBy":
                        throw new InvalidOperationException(
                            "Cannot order in different directions.");
                    case "OrderByDescending":
                        Visit(o.Arguments[0]);
                        _sb.Append("\"sort\":[{");
                        Visit(lambdaBody);
                        _sb.Append(":\"desc\"}");
                        break;
                    case "ThenByDescending":
                        InspectOrdering(o.Arguments[0]);
                        _sb.Append('{');
                        Visit(lambdaBody);
                        _sb.Append(":\"desc\"}");
                        break;
                    default:
                        return;
                }
                _sb.Append(',');
            }

            InspectOrdering(m);
            _sb.Length--;
            _sb.Append("],");
            return m;
        }

        private void VisitConstantValue(object constant) {
            if (constant is IQueryable) {
                return;
            }
            if (constant == null) {
                _sb.Append("null");
                return;
            }
            if (Type.GetTypeCode(constant.GetType()) != TypeCode.Object) {
                _sb.Append(JsonConvert.SerializeObject(constant));
                return;
            }
            switch (constant) {
                case IEnumerable enumerable:
                    _sb.Append('[');
                    var needsComma = false;
                    foreach (var item in enumerable) {
                        if (needsComma) {
                            _sb.Append(',');
                        }
                        VisitConstantValue(item);
                        needsComma = true;
                    }
                    _sb.Append(']');
                    break;
                case Guid _:
                    _sb.Append(JsonConvert.SerializeObject(constant));
                    break;
                default:
                    var field = constant.GetType().GetRuntimeFields().SingleOrDefault();
                    if (field == null) {
                        throw new NotSupportedException(
                            $"The constant {constant} is not supported.");
                    }
                    VisitConstantValue(field.GetValue(constant));
                    break;
            }
        }

        private void VisitBinaryCombinationOperator(BinaryExpression b, bool not) {
            void InspectBinaryChildren(BinaryExpression e, ExpressionType nodeType) {
                if (e.Left is BinaryExpression lb && lb.NodeType == nodeType) {
                    InspectBinaryChildren(lb, nodeType);
                    _sb.Append(',');
                    Visit(e.Right);
                    return;
                }
                if (e.Right is BinaryExpression rb && rb.NodeType == nodeType) {
                    Visit(e.Left);
                    _sb.Append(',');
                    InspectBinaryChildren(rb, nodeType);
                    return;
                }
                Visit(e.Left);
                _sb.Append(',');
                Visit(e.Right);
            }

            switch (b.NodeType) {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _sb.Append("\"$and\":[");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    _sb.Append(not ? "\"$nor\":[" : "\"$or\":[");
                    break;
            }
            InspectBinaryChildren(b, b.NodeType);
            _sb.Append(']');
        }

        /// <summary>
        /// Optimize query expression to translate effectively.
        /// </summary>
        private sealed class QueryOptimizer : ExpressionVisitor {

            public QueryOptimizer() {
                _nextWhereCalls = new Queue<MethodCallExpression>();
            }

            public Expression Optimize(Expression e) {
                return Visit(e);
            }

            /// <inheritdoc/>
            protected override Expression VisitMethodCall(MethodCallExpression node) {
                if (!node.Method.IsGenericMethod) {
                    return node;
                }
                if (!Method.IsSupported(node.Method)) {
                    throw new NotSupportedException(
                        $"Method {node.Method.Name} cannot be converter to a valid query.");
                }

                var genericDefinition = node.Method.GetGenericMethodDefinition();

                // Bool member to constants
                if (!_isVisitingWhereMethodOrChild && genericDefinition == Method.Where) {
                    _isVisitingWhereMethodOrChild = true;
                    var whereNode = VisitMethodCall(node);
                    _isVisitingWhereMethodOrChild = false;
                    return whereNode;
                }

                // Multi-Where Optimization
                if (genericDefinition == Method.Where) {
                    if (_nextWhereCalls.Count == 0) {
                        _nextWhereCalls.Enqueue(node);
                        var tail = Visit(node.Arguments[0]);
                        var currentLambda = node.GetLambda();
                        var conditionExpression = Visit(currentLambda.Body);
                        _nextWhereCalls.Dequeue();

                        while (_nextWhereCalls.Count > 0) {
                            var nextWhereBody = Visit(_nextWhereCalls.Dequeue().GetLambda().Body);
                            conditionExpression = Expression.And(nextWhereBody, conditionExpression);
                        }

                        var conditionLambda = Expression.Quote(
                            Expression.Lambda(conditionExpression, currentLambda.Parameters));
                        return Expression.Call(typeof(Queryable), nameof(Queryable.Where),
                            node.Method.GetGenericArguments(), tail, conditionLambda);
                    }

                    _nextWhereCalls.Enqueue(node);
                    return Visit(node.Arguments[0]);
                }
                return base.VisitMethodCall(node);
            }

            /// <inheritdoc/>
            protected override Expression VisitBinary(BinaryExpression expression) {
                if (_isVisitingWhereMethodOrChild &&
                    expression.Right is ConstantExpression c && c.Type == typeof(bool) &&
                    (expression.NodeType == ExpressionType.Equal ||
                        expression.NodeType == ExpressionType.NotEqual)) {
                    return expression;
                }
                return base.VisitBinary(expression);
            }

            /// <inheritdoc/>
            protected override Expression VisitMember(MemberExpression expression) {
                if (IsWhereBooleanExpression(expression)) {
                    return Expression.MakeBinary(ExpressionType.Equal, expression,
                        Expression.Constant(true));
                }
                return base.VisitMember(expression);
            }

            /// <inheritdoc/>
            protected override Expression VisitUnary(UnaryExpression expression) {
                if (expression.NodeType == ExpressionType.Not &&
                    expression.Operand is MemberExpression m && IsWhereBooleanExpression(m)) {
                    return Expression.MakeBinary(ExpressionType.Equal, m,
                        Expression.Constant(false));
                }
                return base.VisitUnary(expression);
            }

            private bool IsWhereBooleanExpression(MemberExpression expression) {
                return _isVisitingWhereMethodOrChild &&
                       expression.Member is PropertyInfo info &&
                       info.PropertyType == typeof(bool);
            }

            private bool _isVisitingWhereMethodOrChild;
            private readonly Queue<MethodCallExpression> _nextWhereCalls;
        }

        internal static class Method {

            internal static MethodInfo Where { get; }
            internal static MethodInfo Select { get; }
            internal static MethodInfo Skip { get; }
            internal static MethodInfo Take { get; }
            internal static MethodInfo OrderBy { get; }
            internal static MethodInfo OrderByDescending { get; }
            internal static MethodInfo ThenBy { get; }
            internal static MethodInfo ThenByDescending { get; }

            internal static MethodInfo All { get; }
            internal static MethodInfo AnyWithPredicate { get; }
            internal static MethodInfo EnumerableContains { get; }
            internal static MethodInfo In { get; }
            internal static MethodInfo IsMatch { get; }
            internal static MethodInfo Contains { get; }

            private static List<MethodInfo> Supported { get; }

            internal static bool IsSupported(MethodInfo methodInfo) {
                if (methodInfo.IsGenericMethod) {
                    methodInfo = methodInfo.GetGenericMethodDefinition();
                }
                return Supported.Contains(methodInfo);
            }

            static Method() {
                var queryableMethods = typeof(Queryable)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();

                Where = queryableMethods.Single(
                    mi => mi.Name == nameof(Queryable.Where)
                        && mi.GetParameters().Length == 2
                        && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
                Select = queryableMethods.Single(
                    mi => mi.Name == nameof(Queryable.Select)
                        && mi.GetParameters().Length == 2
                        && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
                Skip = queryableMethods.Single(
                    mi => mi.Name == nameof(Queryable.Skip) && mi.GetParameters().Length == 2);
                Take = queryableMethods.Single(
                    mi => mi.Name == nameof(Queryable.Take) && mi.GetParameters().Length == 2);
                OrderBy = queryableMethods.Single(
                    mi => mi.Name == nameof(Queryable.OrderBy)
                        && mi.GetParameters().Length == 2
                        && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
                OrderByDescending = queryableMethods.Single(
                    mi => mi.Name == nameof(Queryable.OrderByDescending)
                        && mi.GetParameters().Length == 2
                        && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
                ThenBy = queryableMethods.Single(
                    mi => mi.Name == nameof(Queryable.ThenBy)
                        && mi.GetParameters().Length == 2
                        && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
                ThenByDescending = queryableMethods.Single(
                    mi => mi.Name == nameof(Queryable.ThenByDescending)
                        && mi.GetParameters().Length == 2
                        && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));

                static bool IsExpressionOfFunc(Type type, int funcGenericArgs = 2) =>
                    type.IsExpressionOfFunc(funcGenericArgs);

                var enumerableMethods = typeof(Enumerable)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();
                All = enumerableMethods.Single(
                    mi => mi.Name == nameof(Enumerable.All)
                          && mi.GetParameters().Length == 2);
                AnyWithPredicate = enumerableMethods.Single(
                    mi => mi.Name == nameof(Enumerable.Any)
                          && mi.GetParameters().Length == 2);

                EnumerableContains = typeof(EnumerableQueryExtensions)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Single(mi => mi.Name == nameof(EnumerableQueryExtensions.Contains));

                var objectExtensionsMethods = typeof(ObjectQueryExtensions)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();
                In = objectExtensionsMethods.Single(mi => mi.Name == nameof(ObjectQueryExtensions.In));

                IsMatch = typeof(StringQueryExtensions)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Single(mi => mi.Name == nameof(StringQueryExtensions.IsMatch));

                Contains = typeof(Enumerable)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Single(mi => mi.Name == nameof(Enumerable.Contains) && mi.GetParameters().Length == 2);

                Supported = new List<MethodInfo> {
                    Where,
                    OrderBy,
                    ThenBy,
                    OrderByDescending,
                    ThenByDescending,
                    Skip,
                    Take,
                    Select,
                    All,
                    AnyWithPredicate,
                    EnumerableContains,
                    In,
                    IsMatch,
                    Contains
                };
            }
        }

        private readonly StringBuilder _sb;
        private bool _isSelectorSet;
        public bool _inWhere;
    }
}
