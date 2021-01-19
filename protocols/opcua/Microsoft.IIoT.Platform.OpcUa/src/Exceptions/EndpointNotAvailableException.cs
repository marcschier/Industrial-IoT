// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.OpcUa.Exceptions {
    using Microsoft.IIoT.Platform.Core.Models;
    using System;

    /// <summary>
    /// Exception when an endpoint with requested security settings
    /// is not available.
    /// </summary>
    public class EndpointNotAvailableException : Exception {

        /// <inheritdoc/>
        public EndpointNotAvailableException() {
        }

        /// <inheritdoc/>
        public EndpointNotAvailableException(string message) :
            base(message) {
        }

        /// <inheritdoc/>
        public EndpointNotAvailableException(string message,
            Exception innerException) : base(message, innerException) {
        }

        /// <summary>
        /// Create EndpointNotAvailableException.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="securityMode"></param>
        /// <param name="securityPolicy"></param>
        public EndpointNotAvailableException(string endpoint,
            SecurityMode? securityMode, string securityPolicy) :
            this($"There is not endpoint with requested security settings " +
                $"(SecurityMode: {securityMode}, " +
                $"SecurityPolicyUrl: {securityPolicy}) " +
                $"available at url '{endpoint}'.") {
        }
    }
}
