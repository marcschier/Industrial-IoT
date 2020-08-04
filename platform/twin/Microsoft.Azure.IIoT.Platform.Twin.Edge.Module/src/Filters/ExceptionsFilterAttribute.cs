// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Edge.Module.Filters {
    using Microsoft.Azure.IIoT.Platform.Exceptions;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Exceptions;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Security;
    using System.Threading.Tasks;

    /// <summary>
    /// Convert all the exceptions returned by the module controllers to a
    /// status code.
    /// </summary>
    public class ExceptionsFilterAttribute : ExceptionFilterAttribute {

        /// <inheritdoc />
        public override Exception Filter(Exception exception, out int status) {
            switch (exception) {
                case AggregateException ae:
                    var root = ae.GetBaseException();
                    if (!(root is AggregateException)) {
                        return Filter(root, out status);
                    }
                    ae = root as AggregateException;
                    status = (int)HttpStatusCode.InternalServerError;
                    Exception result = null;
                    foreach (var ex in ae.InnerExceptions) {
                        result = Filter(ex, out status);
                        if (status != (int)HttpStatusCode.InternalServerError) {
                            break;
                        }
                    }
                    return result;
                case ResourceNotFoundException _:
                    status = (int)HttpStatusCode.NotFound;
                    break;
                case ResourceInvalidStateException _:
                    status = (int)HttpStatusCode.Forbidden;
                    break;
                case ConflictingResourceException _:
                    status = (int)HttpStatusCode.Conflict;
                    break;
                case SecurityException _:
                case UnauthorizedAccessException _:
                    status = (int)HttpStatusCode.Unauthorized;
                    break;
                case SerializerException _:
                case MethodCallException _:
                case BadRequestException _:
                case ArgumentException _:
                    status = (int)HttpStatusCode.BadRequest;
                    break;
                case NotImplementedException _:
                case NotSupportedException _:
                    status = (int)HttpStatusCode.NotImplemented;
                    break;
                case TimeoutException _:
                    status = (int)HttpStatusCode.RequestTimeout;
                    break;
                case SocketException _:
                case CommunicationException _:
                    status = (int)HttpStatusCode.BadGateway;
                    break;
                case MessageSizeLimitException _:
                    status = (int)HttpStatusCode.RequestEntityTooLarge;
                    break;
                case TaskCanceledException _:
                case OperationCanceledException _:
                    status = (int)HttpStatusCode.Gone;
                    break;

                //
                // The following will most certainly be retried by our
                // service client implementations and thus dependent
                // services:
                //
                //      InternalServerError
                //      GatewayTimeout
                //      PreconditionFailed
                //      TemporaryRedirect
                //      TooManyRequests
                //
                // As such, if you want to terminate make sure exception
                // is caught ahead of here and returns a status other than
                // one of the above.
                //

                case ServerBusyException _:
                    status = (int)HttpStatusCode.TooManyRequests;
                    break;
                case ExternalDependencyException _:
                    status = (int)HttpStatusCode.ServiceUnavailable;
                    break;
                case ResourceOutOfDateException _:
                    status = (int)HttpStatusCode.PreconditionFailed;
                    break;
                default:
                    status = (int)HttpStatusCode.InternalServerError;
                    break;
            }
            return exception;
        }
    }
}
