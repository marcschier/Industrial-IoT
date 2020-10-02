// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Http.Exceptions {
    using Microsoft.Azure.IIoT.Exceptions;
    using System;
    using System.Net;

    /// <summary>
    /// Retriable exception
    /// </summary>
    public class HttpTransientException : HttpResponseException,
        ITransientException {

        /// <inheritdoc />
        public HttpTransientException() {
        }

        /// <inheritdoc />
        public HttpTransientException(string message) :
            base(message) {
        }

        /// <inheritdoc />
        public HttpTransientException(string message,
            Exception innerException) : base(message, innerException) {
        }

        /// <inheritdoc />
        public HttpTransientException(HttpStatusCode statusCode) :
            base(statusCode) {
        }

        /// <inheritdoc />
        public HttpTransientException(HttpStatusCode statusCode, string message) :
            base(statusCode, message) {
        }
    }
}
