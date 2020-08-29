// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Models {
    public class PublisherInfoRequested {

        public PublisherInfoRequested(PublisherInfo publisher) {
            RequestedLogLevel = publisher?.PublisherModel?.LogLevel?.ToString();
        }

        /// <summary>
        /// Requested log level
        /// </summary>
        public string RequestedLogLevel { get; internal set; }
    }
}
