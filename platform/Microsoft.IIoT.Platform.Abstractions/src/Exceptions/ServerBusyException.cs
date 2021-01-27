// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Exceptions {
    using Microsoft.IIoT.Exceptions;
    using System;

    /// <summary>
    /// Thrown when failing to connect to resource
    /// </summary>
    public class ServerBusyException : ExternalDependencyException {

        /// <inheritdoc/>
        public ServerBusyException() {
        }

        /// <inheritdoc/>
        public ServerBusyException(string message) :
            base(message) {
        }

        /// <inheritdoc/>
        public ServerBusyException(string message, Exception innerException) :
            base(message, innerException) {
        }
    }
}