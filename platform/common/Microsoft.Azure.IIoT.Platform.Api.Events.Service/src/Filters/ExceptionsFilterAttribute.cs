// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Api.Events.Service.Filters {
    using Microsoft.Azure.IIoT.Platform.Exceptions;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security;
    using System.Threading.Tasks;

    /// <summary>
    /// Detect all the unhandled exceptions returned by the API controllers
    /// and decorate the response accordingly, managing the HTTP status code
    /// and preparing a JSON response with useful error details.
    /// When including the stack trace, split the text in multiple lines
    /// for an easier parsing.
    /// @see https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters
    /// </summary>
    public sealed class ExceptionsFilterAttribute : ExceptionFilterAttribute {

        /// <inheritdoc />
        public override void OnException(ExceptionContext context) {
            if (context is null) {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Exception == null) {
                base.OnException(context);
                return;
            }
            if (context.Exception is AggregateException ae) {
                var root = ae.GetBaseException();
                if (root is AggregateException) {
                    context.Exception = ae.InnerExceptions.First();
                }
                else {
                    context.Exception = root;
                }
            }
            switch (context.Exception) {
                case ResourceNotFoundException _:
                    context.Result = GetResponse(HttpStatusCode.NotFound,
                        context.Exception);
                    break;
                case ResourceInvalidStateException _:
                    context.Result = GetResponse(HttpStatusCode.Forbidden,
                        context.Exception);
                    break;
                case ResourceConflictException _:
                    context.Result = GetResponse(HttpStatusCode.Conflict,
                        context.Exception);
                    break;
                case UnauthorizedAccessException _:
                case SecurityException _:
                    context.Result = GetResponse(HttpStatusCode.Unauthorized,
                        context.Exception);
                    break;
                case MethodCallStatusException mcs:
                    context.Result = GetResponse((HttpStatusCode)mcs.Result,
                        context.Exception);
                    break;
                case SerializerException _:
                case MethodCallException _:
                case BadRequestException _:
                case ArgumentException _:
                    context.Result = GetResponse(HttpStatusCode.BadRequest,
                        context.Exception);
                    break;
                case NotImplementedException _:
                case NotSupportedException _:
                    context.Result = GetResponse(HttpStatusCode.NotImplemented,
                        context.Exception);
                    break;
                case TimeoutException _:
                    context.Result = GetResponse(HttpStatusCode.RequestTimeout,
                        context.Exception);
                    break;
                case SocketException _:
                case CommunicationException _:
                    context.Result = GetResponse(HttpStatusCode.BadGateway,
                        context.Exception);
                    break;

                //
                // The following will most certainly be retried by our
                // service client implementations and thus dependent
                // services:
                //
                //      InternalServerError
                //      BadGateway
                //      ServiceUnavailable
                //      GatewayTimeout
                //      PreconditionFailed
                //      TemporaryRedirect
                //      429 (IoT Hub throttle)
                //
                // As such, if you want to terminate make sure exception
                // is caught ahead of here and returns a status other than
                // one of the above.
                //

                case ServerBusyException _:
                    context.Result = GetResponse((HttpStatusCode)429,
                        context.Exception);
                    break;
                case ResourceOutOfDateException _:
                    context.Result = GetResponse(HttpStatusCode.PreconditionFailed,
                        context.Exception);
                    break;
                case ExternalDependencyException _:
                    context.Result = GetResponse(HttpStatusCode.ServiceUnavailable,
                        context.Exception);
                    break;
                default:
                    context.Result = GetResponse(HttpStatusCode.InternalServerError,
                        context.Exception);
                    break;
            }
        }

        /// <inheritdoc />
        public override Task OnExceptionAsync(ExceptionContext context) {
            try {
                OnException(context);
                return Task.CompletedTask;
            }
            catch (Exception) {
                return base.OnExceptionAsync(context);
            }
        }

        /// <summary>
        /// Create result
        /// </summary>
        /// <param name="code"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private ObjectResult GetResponse(HttpStatusCode code, Exception exception) {
            var result = new ObjectResult(exception) {
                StatusCode = (int)code
            };
            return result;
        }
    }
}
