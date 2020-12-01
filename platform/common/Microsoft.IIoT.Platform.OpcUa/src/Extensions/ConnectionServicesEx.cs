// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.OpcUa {
    using Microsoft.IIoT.Platform.Core.Models;
    using System.Threading.Tasks;
    using System;
    using Opc.Ua.Client;
    using System.Threading;

    /// <summary>
    /// Connection services extensions
    /// </summary>
    public static class ConnectionServicesEx {

        /// <summary>
        /// Overload that does not continue on exception and can only be cancelled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="ct"></param>
        /// <param name="connection"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static Task<T> ExecuteServiceAsync<T>(this IConnectionServices client,
            ConnectionModel connection, Func<Session, Task<T>> service,
            CancellationToken ct = default) {
            return client.ExecuteServiceAsync(connection, service, _ => true, ct);
        }

        /// <summary>
        /// Overload which can only be cancelled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="ct"></param>
        /// <param name="connection"></param>
        /// <param name="service"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static Task<T> ExecuteServiceAsync<T>(this IConnectionServices client,
            ConnectionModel connection, Func<Session, Task<T>> service, 
            Func<Exception, bool> handler, CancellationToken ct = default) {
            return client.ExecuteServiceAsync(connection, null, 0, service,
                null, handler, ct);
        }

        /// <summary>
        /// Execute the service on the provided session.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="connection"></param>
        /// <param name="priority"></param>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<T> ExecuteServiceAsync<T>(this IConnectionServices client,
            ConnectionModel connection, int priority, Func<Session, Task<T>> service, 
            CancellationToken ct = default) {
            return client.ExecuteServiceAsync(connection, null, priority, service, 
                null, _ => true, ct);
        }

        /// <summary>
        /// Execute the service on the provided session.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="connection"></param>
        /// <param name="elevation"></param>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<T> ExecuteServiceAsync<T>(this IConnectionServices client,
            ConnectionModel connection, CredentialModel elevation, 
            Func<Session, Task<T>> service, CancellationToken ct = default) {
            return client.ExecuteServiceAsync(connection, elevation, 0, service, 
                null, _ => true, ct);
        }

        /// <summary>
        /// Execute the service on the provided session.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="endpoint"></param>
        /// <param name="elevation"></param>
        /// <param name="priority"></param>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        /// <param name="exceptionHandler"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<T> ExecuteServiceAsync<T>(this IConnectionServices client,
            EndpointModel endpoint, CredentialModel elevation, int priority, 
            Func<Session, Task<T>> service, TimeSpan? timeout, 
            Func<Exception, bool> exceptionHandler, CancellationToken ct = default) {
            return client.ExecuteServiceAsync(endpoint.ToConnectionModel(),
                elevation, priority, service, timeout, exceptionHandler, ct);
        }

        /// <summary>
        /// Overload that runs in the foreground, does not continue on exception
        /// times out default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="endpoint"></param>
        /// <param name="elevation"></param>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<T> ExecuteServiceAsync<T>(this IConnectionServices client,
            EndpointModel endpoint, CredentialModel elevation, 
            Func<Session, Task<T>> service, CancellationToken ct = default) {
            return client.ExecuteServiceAsync(endpoint, elevation, 0, service,
                null, _ => true, ct);
        }
    }
}
