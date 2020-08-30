﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Exceptions {
    using Microsoft.Azure.IIoT.Exceptions;

    /// <summary>
    /// Subscription already exists
    /// </summary>
    public class SubscriptionAlreadyExistsException : ResourceConflictException {
        /// <inheritdoc/>
        public SubscriptionAlreadyExistsException() {
        }

        /// <inheritdoc/>
        public SubscriptionAlreadyExistsException(string subscriptionName) : base(
            $"The subscription '{subscriptionName}' already exists.") {
        }
    }
}