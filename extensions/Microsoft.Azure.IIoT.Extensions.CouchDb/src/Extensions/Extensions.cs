// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace System {
    using Microsoft.Azure.IIoT.Exceptions;
    using Newtonsoft.Json;
    using Flurl.Http;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Runtime.Serialization;
    using System.Linq.Expressions;

    internal static class Extensions {

        /// <summary>
        /// Get property name
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        internal static string GetPropertyName(this MemberInfo memberInfo) {
            var datamember = memberInfo.GetCustomAttribute<DataMemberAttribute>(true);
            var name = datamember?.Name ?? memberInfo.Name;
            if (name == "id") {
                name = "_id"; // Translate from convention to couchdb id property
            }
            return name;
        }

        internal static bool IsExpressionOfFunc(this Type type, int funcGenericArgs = 2) {
            return type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Expression<>) &&
                type.GetGenericArguments()[0].IsGenericType &&
                type.GetGenericArguments()[0].GetGenericArguments().Length == funcGenericArgs;
        }

        internal static bool IsSelector<T>(this Type type) {
            return type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Expression<>) &&
                type.GetGenericArguments()[0].IsGenericType &&
                type.GetGenericArguments()[0].GetGenericArguments().Length == 2 &&
                type.GetGenericArguments()[0].GetGenericArguments()[1] == typeof(T);
        }

        /// <summary>
        /// Get lambda expression from method call
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal static LambdaExpression GetLambda(this MethodCallExpression node) {
            var e = node.Arguments[1];
            while (e.NodeType == ExpressionType.Quote) {
                e = ((UnaryExpression)e).Operand;
            }
            return (LambdaExpression)e;
        }

        /// <summary>
        /// Send request
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="asyncRequest"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task<TResult> SendRequestAsync<TResult>(
            this Task<TResult> asyncRequest, string message = null) {
            try {
                return await asyncRequest.ConfigureAwait(false);
            }
            catch (FlurlHttpException ex) {
                var couchError = await ex.GetResponseJsonAsync<OperationError>()
                    .ConfigureAwait(false) ?? new OperationError();
                throw ex.Call.HttpStatus switch
                {
                    HttpStatusCode.Conflict => new ResourceConflictException(
                        couchError.ToString(message), ex),
                    HttpStatusCode.NotFound => new ResourceNotFoundException(
                        couchError.ToString(message), ex),
                    HttpStatusCode.BadRequest when couchError.Error == "no_usable_index" =>
                        new ResourceNotFoundException(couchError.ToString(message), ex),
                    HttpStatusCode.BadRequest when couchError.Reason == "Invalid rev format" =>
                        new ResourceOutOfDateException(couchError.ToString(message), ex),
                    _ => new ExternalDependencyException(couchError.ToString(message), ex)
                };
            }
        }

        /// <summary>
        /// Couch error response
        /// </summary>
        internal class OperationError {
            /// <summary> Error </summary>
            [JsonProperty("error")]
            public string Error { get; set; }
            /// <summary> Reason </summary>
            [JsonProperty("reason")]
            public string Reason { get; set; }
            /// <inheritdoc/>
            public string ToString(string extra) {
                var message = $"{Error}: {Reason}";
                if (extra != null) {
                    message += $"\n{extra}";
                }
                return message;
            }
        }
    }
}
