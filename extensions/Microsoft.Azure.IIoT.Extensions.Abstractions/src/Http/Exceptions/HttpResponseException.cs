// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Http.Exceptions {
    using System;
    using System.Net;

    /// <summary>
    /// Http request exception
    /// </summary>
    public class HttpResponseException : Exception {

        /// <inheritdoc/>
        public HttpResponseException() {
            StatusCode = HttpStatusCode.InternalServerError;
        }

        /// <inheritdoc/>
        public HttpResponseException(string message) :
            base(message) {
            StatusCode = HttpStatusCode.InternalServerError;
        }

        /// <inheritdoc/>
        public HttpResponseException(string message, Exception innerException) :
            base(message, innerException) {
            StatusCode = HttpStatusCode.InternalServerError;
        }

        /// <inheritdoc/>
        public HttpResponseException(HttpStatusCode statusCode) {
            StatusCode = statusCode;
        }

        /// <inheritdoc/>
        public HttpResponseException(HttpStatusCode statusCode, string message) :
            this(message) {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Response status code
        /// </summary>
        public HttpStatusCode StatusCode { get; }
    }
}
