// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Exceptions {
    using System;

    /// <summary>
    /// This exception is thrown when method call returned a
    /// status other than 200
    /// </summary>
    public class MethodCallStatusException : MethodCallException {

        /// <summary>
        /// Result of method call
        /// </summary>
        public int Result { get; }

        /// <summary>
        /// Payload
        /// </summary>
        public string ResponsePayload { get; }

        /// <inheritdoc/>
        public MethodCallStatusException() :
            this(500, "") {
        }

        /// <inheritdoc/>
        public MethodCallStatusException(string message) :
            this(500, message) {
        }

        /// <inheritdoc/>
        public MethodCallStatusException(string message, Exception innerException) :
            this(500, message, innerException) {
        }

        /// <summary>
        /// Create exception
        /// </summary>
        /// <param name="result"></param>
        /// <param name="errorMessage"></param>
        /// <param name="innerException"></param>
        public MethodCallStatusException(int result, string errorMessage = null,
            Exception innerException = null) :
            this("{}", result, errorMessage, innerException) {
        }

        /// <summary>
        /// Create exception
        /// </summary>
        /// <param name="responsePayload"></param>
        /// <param name="result"></param>
        /// <param name="errorMessage"></param>
        /// <param name="innerException"></param>
        public MethodCallStatusException(string responsePayload, int result,
            string errorMessage = null, Exception innerException = null) :
            base($"Response {result} {errorMessage ?? ""}: {responsePayload}",
                innerException) {
            Result = result;
            ResponsePayload = responsePayload;
        }
    }
}
