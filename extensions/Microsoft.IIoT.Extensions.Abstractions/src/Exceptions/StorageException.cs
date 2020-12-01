// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Exceptions {
    using System;

    /// <summary>
    /// Thrown when accessing storage systems fails.
    /// </summary>
    public class StorageException : ExternalDependencyException {

        /// <inheritdoc />
        public StorageException() :
            this("Failed a storage operation") {
        }

        /// <inheritdoc />
        public StorageException(string message) : base(message) {
        }

        /// <inheritdoc />
        public StorageException(string message, Exception innerException) :
            base(message, innerException) {
        }
    }
}
