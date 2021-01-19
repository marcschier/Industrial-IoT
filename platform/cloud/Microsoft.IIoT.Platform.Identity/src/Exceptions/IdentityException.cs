// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Exceptions {
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Identity exception
    /// </summary>
    public class IdentityException : Exception {

        /// <summary>
        /// Errors
        /// </summary>
        public IEnumerable<IdentityError> Errors { get; }

        /// <inheritdoc />
        public IdentityException() {
            Errors = new List<IdentityError>();
        }

        /// <inheritdoc />
        public IdentityException(IEnumerable<IdentityError> errors) {
            Errors = errors;
        }

        /// <inheritdoc />
        public IdentityException(string message) : base(message) {
        }

        /// <inheritdoc />
        public IdentityException(string message, Exception innerException) :
            base(message, innerException) {
        }
    }
}
