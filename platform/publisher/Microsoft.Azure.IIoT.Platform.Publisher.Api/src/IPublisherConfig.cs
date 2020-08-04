// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api {

    /// <summary>
    /// Configuration for service
    /// </summary>
    public interface IPublisherConfig {

        /// <summary>
        /// Opc publisher service url
        /// </summary>
        string OpcUaPublisherServiceUrl { get; }
    }
}
