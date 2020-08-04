// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Data set information describing metadata and settings
    /// </summary>
    public class PublishedDataSetSourceInfoModel {

        /// <summary>
        /// Name of the published dataset in the writer
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Endpoint id
        /// </summary>
        public string EndpointId { get; set; }

        /// <summary>
        /// Credential to use if any
        /// </summary>
        public CredentialModel User { get; set; }

        /// <summary>
        /// Source state
        /// </summary>
        public PublishedDataSetSourceStateModel State { get; set; }

        /// <summary>
        /// Subscription settings
        /// </summary>
        public PublishedDataSetSourceSettingsModel SubscriptionSettings { get; set; }

        /// <summary>
        /// Operation timeout
        /// </summary>
        public TimeSpan? OperationTimeout { get; set; }

        /// <summary>
        /// Diagnostics to use on the source
        /// </summary>
        public DiagnosticsLevel? DiagnosticsLevel { get; set; }

        /// <summary>
        /// Extension fields
        /// </summary>
        public Dictionary<string, string> ExtensionFields { get; set; }
    }
}