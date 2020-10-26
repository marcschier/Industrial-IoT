﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {

    /// <summary>
    /// Publisher update request
    /// </summary>
    public class PublisherUpdateModel {

        /// <summary>
        /// Current log level
        /// </summary>
        public TraceLogLevel? LogLevel { get; set; }

        /// <summary>
        /// Generation Id
        /// </summary>
        public string GenerationId { get; set; }
    }
}
